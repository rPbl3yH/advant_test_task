using Game.Configs;

namespace Game.EcsEngine.Systems
{
    public static class PriceUseCases
    {
        public static int GetNextLevelPrice(int currentLevel, BusinessConfig businessConfig)
        {
            return (currentLevel + 1) * businessConfig.BaseCost;
        }
    }
}