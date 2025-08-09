using System.Globalization;
using Game.App;
using Game.Configs;
using Game.EcsEngine.Components;
using Game.Gameplay;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Game.EcsEngine.Systems
{
    public sealed class LevelUpRequestSystem : IEcsRunSystem
    {
        private readonly EcsCustomInject<CurrencyStorage> _currencyStorage;
        private readonly EcsWorldInject _eventsWorld = EcsWorlds.EVENTS;

        private readonly EcsCustomInject<ConfigsSharedData> _configurationData = default;
        private readonly EcsFilterInject<Inc<LevelUpRequest>> _levelUpRequestFilter = EcsWorlds.EVENTS;

        private readonly EcsFilterInject<Inc<BusinessUpgradePurchasedComponent, BusinessComponent>> _purchasedFilter = default;

        public void Run(IEcsSystems systems)
        {
            var gameWorld = systems.GetWorld();

            var businessPool = gameWorld.GetPool<BusinessComponent>();
            var levelPool = gameWorld.GetPool<LevelComponent>();
            var viewPool = gameWorld.GetPool<BusinessViewComponent>();

            var purchasedPool = gameWorld.GetPool<BusinessUpgradePurchasedComponent>();

            foreach (int requestEntity in _levelUpRequestFilter.Value)
            {
                ref LevelUpRequest request = ref _levelUpRequestFilter.Pools.Inc1.Get(requestEntity);
                string businessId = request.BusinessId;

                foreach (int entity in gameWorld.Filter<BusinessComponent>().End())
                {
                    if (businessPool.Get(entity).Id != businessId) continue;

                    ref LevelComponent level = ref levelPool.Get(entity);
                    ref BusinessViewComponent view = ref viewPool.Get(entity);

                    var config = _configurationData.Value.BusinessConfigs[businessId];
                    int nextPrice = PriceUseCases.GetNextLevelPrice(level.Level, config);
                    if (!_currencyStorage.Value.IsEnough(nextPrice)) break;

                    _currencyStorage.Value.Subtract(nextPrice);
                    level.Level++;

                    view.View.SetLevel(level.Level);
                    view.View.SetLevelUpPrice(PriceUseCases.GetNextLevelPrice(level.Level, config));

                    float multiplier = IncomeUseCases.GetIncomeMultiplier(gameWorld, _purchasedFilter.Value,
                        purchasedPool, businessPool, businessId, _configurationData.Value);

                    int incomeValue = IncomeUseCases.GetIncome(level.Level, config.BaseIncome, multiplier);
                    view.View.SetIncome(incomeValue);
                    
                    GameSaveService.Save();

                    break;
                }

                _eventsWorld.Value.DelEntity(requestEntity);
            }
        }
    }
}