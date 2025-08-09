using Game.Configs;
using Leopotam.EcsLite;

namespace Game.App
{
    public static class GameSaveService
    {
        private static GameSaveManager s_manager;

        public static void Initialize(EcsWorld gameWorld, Game.Gameplay.CurrencyStorage currencyStorage)
        {
            s_manager = new GameSaveManager(gameWorld, currencyStorage);
        }

        public static void Save() => s_manager?.Save();

        public static void Load(ConfigsSharedData configsSharedData) => s_manager?.Load(configsSharedData);

        public static void ClearSave() => s_manager?.Clear();
    }
}