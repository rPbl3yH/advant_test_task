using Game.App;
using Game.Configs;
using Game.EcsEngine.Components;
using Game.Gameplay;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Game.EcsEngine.Systems
{
    public sealed class UpgradeRequestSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject _eventsWorld = EcsWorlds.EVENTS;
        private readonly EcsCustomInject<CurrencyStorage> _currencyStorage;
        private readonly EcsCustomInject<ConfigsSharedData> _configurationData = default;
        private readonly EcsFilterInject<Inc<UpgradeRequest>> _upgradeRequestFilter = EcsWorlds.EVENTS;

        private readonly EcsFilterInject<Inc<BusinessUpgradePurchasedComponent, BusinessComponent>> _purchasedFilter = default;

        public void Run(IEcsSystems systems)
        {
            var gameWorld = systems.GetWorld();

            var businessPool = gameWorld.GetPool<BusinessComponent>();
            var viewPool = gameWorld.GetPool<BusinessViewComponent>();
            var purchasedPool = gameWorld.GetPool<BusinessUpgradePurchasedComponent>();
            var levelPool = gameWorld.GetPool<LevelComponent>();

            foreach (int requestEntity in _upgradeRequestFilter.Value)
            {
                ref UpgradeRequest request = ref _upgradeRequestFilter.Pools.Inc1.Get(requestEntity);
                string businessId = request.BusinessId;
                int upgradeIndex = request.UpgradeIndex;

                foreach (int entity in gameWorld.Filter<BusinessComponent>().End())
                {
                    if (businessPool.Get(entity).Id != businessId)
                    {
                        continue;
                    }

                    var businessConfig = _configurationData.Value.BusinessConfigs[businessId];
                    var upgradeConfig = businessConfig.Upgrades[upgradeIndex];
                    
                    if (!_currencyStorage.Value.IsEnough(upgradeConfig.Price))
                    {
                        break;
                    }

                    _currencyStorage.Value.Subtract(upgradeConfig.Price);

                    int mark = gameWorld.NewEntity();
                    ref var purchased = ref purchasedPool.Add(mark);
                    purchased.Index = upgradeIndex;
                    businessPool.Add(mark).Id = businessId;

                    ref var view = ref viewPool.Get(entity);
                    view.View.UpgradeViews[upgradeIndex].SetUpgradePurchased();

                    ref var level = ref levelPool.Get(entity);
                    float multiplier = IncomeUseCases.GetIncomeMultiplier(gameWorld, _purchasedFilter.Value,
                        purchasedPool, businessPool, businessId, _configurationData.Value);

                    int incomeValue = IncomeUseCases.GetIncome(level.Level, businessConfig.BaseIncome, multiplier);
                    view.View.SetIncome(incomeValue);

                    GameSaveService.Save();
                    
                    break;
                }

                _eventsWorld.Value.DelEntity(requestEntity);
            }
        }
    }
}