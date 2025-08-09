using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(menuName = "Configs/BusinessList")]
    public class BusinessConfigList : ScriptableObject
    {
        public BusinessConfig[] Configs;
    }
}