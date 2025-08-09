using System;
using UnityEngine;

namespace Game.Gameplay
{
    [Serializable]
    public class CurrencyStorage
    {
        public int Value => _value;
        
        [SerializeField]
        private int _value;

        public CurrencyStorage(int initialValue = 150)
        {
            _value = initialValue;
        }

        public void Add(int amount)
        {
            _value += amount;
        }

        public void Subtract(int amount)
        {
            _value -= amount;
        }

        public bool IsEnough(int amount)
        {
            return _value >= amount;
        }
    }   
}