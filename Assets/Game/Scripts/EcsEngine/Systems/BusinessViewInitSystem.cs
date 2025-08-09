using System.Linq;
using Game.Configs;
using Game.EcsEngine.Components;
using Game.EcsEngine.Views;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Game.EcsEngine.Systems
{
    public sealed class BusinessViewInitSystem : IEcsInitSystem
    {
        private readonly BusinessConfig[] _configs;
        private readonly Transform _container;
        private readonly BusinessView _viewPrefab;
        private readonly BusinessUpgradeView _upgradePrefab;

        private readonly EcsCustomInject<ConfigsSharedData> _configsShared = default;
        private readonly EcsCustomInject<RuntimeSharedData> _runtimeShared = default;

        public BusinessViewInitSystem(BusinessConfig[] configs, Transform container,
            BusinessView viewPrefab, BusinessUpgradeView upgradePrefab)
        {
            _configs = configs;
            _container = container;
            _viewPrefab = viewPrefab;
            _upgradePrefab = upgradePrefab;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var businessPool = world.GetPool<BusinessComponent>();
            var levelPool = world.GetPool<LevelComponent>();
            var incomePool = world.GetPool<IncomeComponent>();
            var delayPool = world.GetPool<IncomeDelayComponent>();
            var progressPool = world.GetPool<IncomeProgressComponent>();
            var viewPool = world.GetPool<BusinessViewComponent>();

            foreach (var config in _configs)
            {
                int entity = world.NewEntity();

                ref var business = ref businessPool.Add(entity);
                business.Id = config.BusinessId;

                ref var level = ref levelPool.Add(entity);
                level.Level = config == _configs[0] ? 1 : 0;

                incomePool.Add(entity).BaseIncome = config.BaseIncome;
                delayPool.Add(entity).Delay = config.IncomeDelay;
                progressPool.Add(entity).Timer = 0f;

                ref var viewComp = ref viewPool.Add(entity);
                viewComp.BusinessId = config.BusinessId;

                var view = NTC.Pool.NightPool.Spawn(_viewPrefab, _container);
                view.BusinessNameText.SetText(config.BusinessName);
                view.SetLevel(level.Level);
                view.SetIncome(0);
                view.SetLevelUpPrice(PriceUseCases.GetNextLevelPrice(level.Level, config));

                for (int upgradeIndex = 0; upgradeIndex < config.Upgrades.Count; upgradeIndex++)
                {
                    var upgradeConfig = config.Upgrades[upgradeIndex];
                    var upgradeView = NTC.Pool.NightPool.Spawn(_upgradePrefab, view.UpgradesContainer, false);
                    upgradeView.Root.anchoredPosition = view.UpgradesPoints[upgradeIndex].anchoredPosition;
                    upgradeView.SetUpgradeName(upgradeConfig.UpgradeName);
                    var percent = Mathf.RoundToInt(upgradeConfig.IncomeMultiplier * 100);
                    upgradeView.SetUpgradeDescriptionPercent(percent);
                    upgradeView.SetUpgradePrice(upgradeConfig.Price);
                    view.UpgradeViews.Add(upgradeView);
                }

                viewComp.View = view;

                _runtimeShared.Value.EntitiesByBusinessId[config.BusinessId] = entity;
            }

            _configsShared.Value.BusinessConfigs = _configs.ToDictionary(c => c.BusinessId, c => c);
        }
    }
}