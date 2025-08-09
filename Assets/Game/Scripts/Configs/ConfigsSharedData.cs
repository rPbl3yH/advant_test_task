using System;
using System.Collections.Generic;

namespace Game.Configs
{
    [Serializable]
    public sealed class ConfigsSharedData
    {
        public Dictionary<string, BusinessConfig> BusinessConfigs;
    }

    public sealed class RuntimeSharedData
    {
        public readonly Dictionary<string, int> EntitiesByBusinessId = new();
    }
}