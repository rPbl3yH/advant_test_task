using Game.EcsEngine.Views;
using Game.Gameplay;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Game.EcsEngine.Systems
{
    public class CurrencyViewSystem : IEcsRunSystem
    {
        private readonly CurrencyView _currencyView;
        private readonly EcsCustomInject<CurrencyStorage> _currencyStorage;

        public CurrencyViewSystem(CurrencyView currencyView)
        {
            _currencyView = currencyView;
        }

        public void Run(IEcsSystems systems)
        {
            _currencyView.CurrencyText.SetText(UITextFormatter.FormatBalanceText(_currencyStorage.Value.Value));
        }
    }
}