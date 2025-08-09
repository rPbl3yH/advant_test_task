using Game.Configs;
using Game.EcsEngine.Components;
using Game.Gameplay;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Game.EcsEngine.Systems
{
    public class BusinessUpgradePurchaseSystem : IEcsRunSystem
    {
        private readonly EcsCustomInject<CurrencyStorage> _currencyStorage;
        private readonly EcsCustomInject<ConfigsSharedData> _sharedData = default;

        private readonly EcsFilterInject<Inc<BusinessUpgradeComponent, BusinessComponent, BusinessViewComponent>> _upgradeFilter = default;
        private readonly EcsPoolInject<BusinessUpgradePurchasedComponent> _purchasedPool = default;
        private readonly EcsPoolInject<BusinessComponent> _businessPool = default;
        
        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _upgradeFilter.Value)
            {
                ref var upgradeComponent = ref _upgradeFilter.Pools.Inc1.Get(entity);
                ref var business = ref _upgradeFilter.Pools.Inc2.Get(entity);
                ref var viewComponent = ref _upgradeFilter.Pools.Inc3.Get(entity);

                var config = _sharedData.Value.BusinessConfigs[business.Id];
                var upgradeConfig = config.Upgrades[upgradeComponent.Index];

                if (!_currencyStorage.Value.IsEnough(upgradeConfig.Price))
                {
                    continue;
                }

                _currencyStorage.Value.Subtract(upgradeConfig.Price);

                var world = _upgradeFilter.Value.GetWorld();
                int purchasedEntity = world.NewEntity();
                ref var purchased = ref _purchasedPool.Value.Add(purchasedEntity);
                purchased.Index = upgradeComponent.Index;
                _businessPool.Value.Add(purchasedEntity).Id = business.Id;

                var upgradeView = viewComponent.View.UpgradeViews[upgradeComponent.Index];
                upgradeView.UpgradePriceText.SetText("Куплено");
                upgradeView.PurchaseButton.interactable = false;

                world.DelEntity(entity);
            }
        }
    }
}