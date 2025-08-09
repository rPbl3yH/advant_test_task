using Game.EcsEngine.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.EcsEngine.Views
{
    public class BusinessView : MonoBehaviour
    {
        public TMP_Text BusinessNameText;
        public TMP_Text LevelText;
        public TMP_Text IncomeText;
        public TMP_Text LevelUpPriceText;
        public Button LevelUpButton;
        public Image IncomeProgressBar;
        public RectTransform[] UpgradesPoints;
        public Transform UpgradesContainer;
        public System.Collections.Generic.List<BusinessUpgradeView> UpgradeViews = new();

        public void SetLevel(int level)
        {
            LevelText.SetText(level.ToString());
        }

        public void SetIncome(int income)
        {
            IncomeText.SetText(UITextFormatter.FormatCurrency(income));
        }

        public void SetLevelUpPrice(int price)
        {
            LevelUpPriceText.SetText(UITextFormatter.FormatPrice(price));
        }
    }
}