namespace Game.EcsEngine.Views
{
    public static class UITextFormatter
    {
        public static string FormatCurrency(int value)
        {
            return $"{value}$";
        }

        public static string FormatPrice(int value)
        {
            return $"Цена: {value}$";
        }

        public static string FormatUpgradeDescriptionPercent(int percent)
        {
            return $"Доход: +{percent}%";
        }

        public static string FormatBalanceText(int value)
        {
            return $"Баланс: {value}$";
        }
    }
}