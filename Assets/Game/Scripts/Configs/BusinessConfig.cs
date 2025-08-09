using System.Collections.Generic;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(menuName = "Configs/Business")]
    public class BusinessConfig : ScriptableObject
    {
        public string BusinessId;
        public string BusinessName;
        public float IncomeDelay;
        public int BaseCost;
        public int BaseIncome;

        public List<BusinessUpgradeConfig> Upgrades = new List<BusinessUpgradeConfig>();
    }
}