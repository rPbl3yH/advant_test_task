using System.Linq;
using Game.App;
using Game.Configs;
using Game.EcsEngine.Systems;
using Game.EcsEngine.Views;
using Game.Gameplay;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.EcsEngine
{
    public class EcsStartup : MonoBehaviour
    {
        [SerializeField] private CurrencyView _currencyView;
        [SerializeField] private BusinessView _businessViewPrefab;
        [SerializeField] private BusinessUpgradeView _upgradeViewPrefab;
        [SerializeField] private Transform _container;
        [SerializeField] private BusinessConfigList _businessConfigsList;
        
        private CurrencyStorage _currencyStorage;
        
        [ShowInInspector, ReadOnly]
        private ConfigsSharedData _sharedData;
        [ShowInInspector, ReadOnly]
        private RuntimeSharedData _runtimeData;
        
        private EcsWorld _gameWorld;
        private EcsWorld _eventsWorld;
        private EcsSystems _gameSystems;
        
        private GameSaveManager _gameSaveManager;

        private void Awake()
        {
            _currencyStorage = new CurrencyStorage();
            _eventsWorld = new EcsWorld();
            _gameWorld = new EcsWorld();
            _gameSaveManager = new GameSaveManager(_gameWorld, _currencyStorage);

            _gameSystems = new EcsSystems(_gameWorld);
            _gameSystems.AddWorld(_eventsWorld, EcsWorlds.EVENTS);
            
            _gameSystems
                .Add(new BusinessViewInitSystem(_businessConfigsList.Configs, _container, _businessViewPrefab, 
                    _upgradeViewPrefab))
                .Add(new BusinessIncomeSystem())
                .Add(new BusinessUpgradePurchaseSystem())
                .Add(new CurrencyViewSystem(_currencyView))
                .Add(new LevelUpRequestSystem())
                .Add(new UpgradeRequestSystem())
                .Add(new BusinessViewClickSystem())
#if UNITY_EDITOR
                .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                ;
            
        }

        void Start()
        {
            _runtimeData = new RuntimeSharedData();
            _sharedData = new ConfigsSharedData {
                BusinessConfigs = _businessConfigsList.Configs.ToDictionary(c => c.BusinessId, c => c)
            };

            GameSaveService.Initialize(_gameWorld, _currencyStorage);

            _gameSystems
                .Inject(_sharedData, _runtimeData, _gameSaveManager, _currencyStorage)
                .Init();
            
            GameSaveService.Load(_sharedData);
        }

        void Update()
        {
            _gameSystems.Run();
        }

        void OnDestroy()
        {
            _gameSystems.Destroy();
            _gameWorld.Destroy();

            _eventsWorld.Destroy();
        }
    }
}