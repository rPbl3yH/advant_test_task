using Game.Configs;
using Game.EcsEngine.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.EcsEngine.Systems
{
    public static class IncomeUseCases
    {
        public static float GetIncomeMultiplier(EcsWorld world,
            EcsFilter purchasedFilter,
            EcsPool<BusinessUpgradePurchasedComponent> purchasedPool,
            EcsPool<BusinessComponent> businessPool,
            string businessId,
            ConfigsSharedData shared)
        {
            float multiplier = 1f;
            foreach (int e in purchasedFilter)
            {
                ref var purchased = ref purchasedPool.Get(e);
                ref var purchasedBusiness = ref businessPool.Get(e);
                if (purchasedBusiness.Id != businessId) continue;

                var upCfg = shared.BusinessConfigs[businessId].Upgrades[purchased.Index];
                multiplier += upCfg.IncomeMultiplier;
            }
            return multiplier;
        }

        public static int GetIncome(int level, int baseIncome, float multiplier)
        {
            return Mathf.RoundToInt(level * baseIncome * multiplier);
        }
    }
}