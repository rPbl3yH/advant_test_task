using System;
using System.Collections.Generic;
using System.IO;
using Game.Configs;
using Game.EcsEngine.Components;
using Game.EcsEngine.Systems;
using Game.Gameplay;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.App
{
    public class GameSaveManager
    {
        private const string FILE_NAME = "GameSave.json";

        private readonly EcsWorld _gameWorld;
        private readonly CurrencyStorage _currencyStorage;

        public GameSaveManager(EcsWorld gameWorld, CurrencyStorage currencyStorage)
        {
            _gameWorld = gameWorld;
            _currencyStorage = currencyStorage;
        }

        public void Save()
        {
            SaveData data = CollectSaveData();
            WriteToDisk(data);
        }

        public void Load(ConfigsSharedData configsSharedData)
        {
            string full = Path.Combine(Application.persistentDataPath, FILE_NAME);
            
            if (!File.Exists(full))
            {
                return;
            }
            
            try
            {
                string json = File.ReadAllText(full);
                SaveData data = JsonConvert.DeserializeObject<SaveData>(json);
                
                if (data != null)
                {
                    ApplySaveData(data, configsSharedData);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to load save: {exception.Message}");
            }
        }

        public void Clear()
        {
            string full = Path.Combine(Application.persistentDataPath, FILE_NAME);
            
            if (File.Exists(full))
            {
                File.Delete(full);
            }
        }

        private SaveData CollectSaveData()
        {
            var saveData = new SaveData
            {
                CurrencyValue = _currencyStorage.Value,
                Businesses = new List<BusinessData>()
            };

            var businessPool = _gameWorld.GetPool<BusinessComponent>();
            var levelPool = _gameWorld.GetPool<LevelComponent>();
            var progressPool = _gameWorld.GetPool<IncomeProgressComponent>();
            var purchasedPool = _gameWorld.GetPool<BusinessUpgradePurchasedComponent>();

            var realBusinessFilter = _gameWorld
                .Filter<BusinessComponent>()
                .Inc<LevelComponent>()
                .Inc<IncomeProgressComponent>()
                .End();

            var purchasedFilter = _gameWorld
                .Filter<BusinessUpgradePurchasedComponent>()
                .Inc<BusinessComponent>()
                .End();

            foreach (int entity in realBusinessFilter)
            {
                if (!levelPool.Has(entity) || !progressPool.Has(entity) || !businessPool.Has(entity))
                    continue;

                ref var business = ref businessPool.Get(entity);
                ref var level = ref levelPool.Get(entity);
                ref var progress = ref progressPool.Get(entity);

                var purchasedList = new List<int>();
                foreach (int pe in purchasedFilter)
                {
                    ref var purchasedBusiness = ref businessPool.Get(pe);
                    if (purchasedBusiness.Id != business.Id)
                    {
                        continue;
                    }

                    purchasedList.Add(purchasedPool.Get(pe).Index);
                }

                saveData.Businesses.Add(new BusinessData
                {
                    BusinessId = business.Id,
                    Level = level.Level,
                    ProgressTimer = progress.Timer,
                    PurchasedUpgrades = purchasedList
                });
            }

            return saveData;
        }

        private void ApplySaveData(SaveData data, ConfigsSharedData configsSharedData)
        {
            int delta = data.CurrencyValue - _currencyStorage.Value;
            
            if (delta > 0)
            {
                _currencyStorage.Add(delta);
            }
            else if (delta < 0)
            {
                _currencyStorage.Subtract(-delta);
            }

            var businessPool = _gameWorld.GetPool<BusinessComponent>();
            var levelPool = _gameWorld.GetPool<LevelComponent>();
            var progressPool = _gameWorld.GetPool<IncomeProgressComponent>();
            var viewPool = _gameWorld.GetPool<BusinessViewComponent>();
            var purchasedPool = _gameWorld.GetPool<BusinessUpgradePurchasedComponent>();

            var oldPurchased = _gameWorld.Filter<BusinessUpgradePurchasedComponent>().End();
            
            foreach (int entity in oldPurchased)
            {
                _gameWorld.DelEntity(entity);
            }

            foreach (BusinessData businessData in data.Businesses)
            {
                int targetEntity = -1;
                
                foreach (int entity in _gameWorld.Filter<BusinessComponent>().End())
                {
                    if (businessPool.Get(entity).Id == businessData.BusinessId)
                    {
                        targetEntity = entity;
                        break;
                    }
                }

                if (targetEntity == -1)
                {
                    continue;
                }

                ref LevelComponent level = ref levelPool.Get(targetEntity);
                ref IncomeProgressComponent progress = ref progressPool.Get(targetEntity);
                ref BusinessViewComponent view = ref viewPool.Get(targetEntity);

                level.Level = businessData.Level;
                progress.Timer = Mathf.Max(0f, businessData.ProgressTimer);

                foreach (int upIndex in businessData.PurchasedUpgrades)
                {
                    int mark = _gameWorld.NewEntity();
                    ref var purchased = ref purchasedPool.Add(mark);
                    purchased.Index = upIndex;
                    businessPool.Add(mark).Id = businessData.BusinessId;

                    if (view.View != null && upIndex >= 0 && upIndex < view.View.UpgradeViews.Count)
                    {
                        view.View.UpgradeViews[upIndex].SetUpgradePurchased();
                    }
                }

                if (configsSharedData.BusinessConfigs.TryGetValue(businessData.BusinessId, out var businessConfig))
                {
                    view.View.SetLevel(level.Level);
                    int nextPrice = PriceUseCases.GetNextLevelPrice(level.Level, businessConfig);
                    view.View.SetLevelUpPrice(nextPrice);

                    float multiplier = 1f;
                    foreach (int upgradeIndex in businessData.PurchasedUpgrades)
                    {
                        multiplier += businessConfig.Upgrades[upgradeIndex].IncomeMultiplier;
                    }

                    int incomePerTick = Mathf.RoundToInt(level.Level * businessConfig.BaseIncome * multiplier);
                    view.View.SetIncome(incomePerTick);

                    if (businessConfig.IncomeDelay > 0f)
                    {
                        view.View.IncomeProgressBar.fillAmount = Mathf.Clamp01(progress.Timer / businessConfig.IncomeDelay);
                    }
                    else
                    {
                        view.View.IncomeProgressBar.fillAmount = 0f;
                    }
                }
            }
        }

        private void WriteToDisk(SaveData data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            string full = Path.Combine(Application.persistentDataPath, FILE_NAME);
            File.WriteAllText(full, json);
        }
    }

    [Serializable]
    public class SaveData
    {
        public int CurrencyValue;
        public List<BusinessData> Businesses;
    }

    [Serializable]
    public class BusinessData
    {
        public string BusinessId;
        public int Level;
        public float ProgressTimer;
        public List<int> PurchasedUpgrades;
    }
}