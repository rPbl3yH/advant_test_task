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
    public class BusinessIncomeSystem : IEcsRunSystem
    {
        private readonly EcsCustomInject<CurrencyStorage> _currencyStorage;
        private readonly EcsCustomInject<ConfigsSharedData> _sharedData;

        private readonly EcsFilterInject<Inc<BusinessComponent, LevelComponent, IncomeComponent, 
            IncomeDelayComponent, IncomeProgressComponent, BusinessViewComponent>> _incomeFilter = default;

        private readonly EcsFilterInject<Inc<BusinessUpgradePurchasedComponent, BusinessComponent>> _purchasedFilter =
            default;

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _incomeFilter.Value)
            {
                ref BusinessComponent businessComponent = ref _incomeFilter.Pools.Inc1.Get(entity);
                ref LevelComponent levelComponent = ref _incomeFilter.Pools.Inc2.Get(entity);
                ref IncomeComponent incomeComponent = ref _incomeFilter.Pools.Inc3.Get(entity);
                ref IncomeDelayComponent delayComponent = ref _incomeFilter.Pools.Inc4.Get(entity);
                ref IncomeProgressComponent progressComponent = ref _incomeFilter.Pools.Inc5.Get(entity);
                ref BusinessViewComponent viewComponent = ref _incomeFilter.Pools.Inc6.Get(entity);

                if (levelComponent.Level <= 0)
                {
                    progressComponent.Timer = 0f;
                    viewComponent.View.IncomeProgressBar.fillAmount = 0f;
                    viewComponent.View.SetIncome(0);
                    viewComponent.View.SetLevel(0);
                    viewComponent.View.SetLevelUpPrice(
                        PriceUseCases.GetNextLevelPrice(0, _sharedData.Value.BusinessConfigs[businessComponent.Id]));
                    continue;
                }

                progressComponent.Timer += Time.deltaTime;
                float normalized = Mathf.Clamp01(progressComponent.Timer / delayComponent.Delay);
                viewComponent.View.IncomeProgressBar.fillAmount = normalized;

                if (progressComponent.Timer < delayComponent.Delay)
                {
                    continue;
                }

                progressComponent.Timer = 0f;

                float incomeMultiplier = 1f;
                foreach (int purchasedEntity in _purchasedFilter.Value)
                {
                    ref var purchased = ref _purchasedFilter.Pools.Inc1.Get(purchasedEntity);
                    ref var purchasedBusiness = ref _purchasedFilter.Pools.Inc2.Get(purchasedEntity);
                    if (purchasedBusiness.Id != businessComponent.Id) continue;

                    var upCfg = _sharedData.Value.BusinessConfigs[businessComponent.Id].Upgrades[purchased.Index];
                    incomeMultiplier += upCfg.IncomeMultiplier;
                }

                int incomeValue = Mathf.RoundToInt(levelComponent.Level * incomeComponent.BaseIncome * incomeMultiplier);
                _currencyStorage.Value.Add(incomeValue);

                viewComponent.View.SetIncome(incomeValue);
                viewComponent.View.SetLevel(levelComponent.Level);
                viewComponent.View.SetLevelUpPrice(PriceUseCases.GetNextLevelPrice
                    (levelComponent.Level, _sharedData.Value.BusinessConfigs[businessComponent.Id]));
                
                GameSaveService.Save();
            }
        }
    }
}