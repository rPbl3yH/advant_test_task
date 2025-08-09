using Game.EcsEngine.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Game.EcsEngine.Systems
{
    public sealed class BusinessViewClickSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject _eventsWorld = EcsWorlds.EVENTS;
        private readonly EcsFilterInject<Inc<BusinessViewComponent>> _viewFilter = default;

        public void Init(IEcsSystems systems)
        {
            foreach (int entity in _viewFilter.Value)
            {
                ref BusinessViewComponent viewComponent = ref _viewFilter.Pools.Inc1.Get(entity);
                string businessId = viewComponent.BusinessId;

                viewComponent.View.LevelUpButton.onClick.AddListener(() =>
                {
                    int e = _eventsWorld.Value.NewEntity();
                    ref LevelUpRequest req = ref _eventsWorld.Value.GetPool<LevelUpRequest>().Add(e);
                    req.BusinessId = businessId;
                });

                for (int i = 0; i < viewComponent.View.UpgradeViews.Count; i++)
                {
                    int upgradeIndex = i;
                    viewComponent.View.UpgradeViews[i].PurchaseButton.onClick.AddListener(() =>
                    {
                        int e = _eventsWorld.Value.NewEntity();
                        ref UpgradeRequest req = ref _eventsWorld.Value.GetPool<UpgradeRequest>().Add(e);
                        req.BusinessId = businessId;
                        req.UpgradeIndex = upgradeIndex;
                    });
                }
            }
        }
    }
}