using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.EcsEngine.Views
{
    public class BusinessUpgradeView : MonoBehaviour
    {
        public RectTransform Root;
        public TMP_Text UpgradeNameText;
        public TMP_Text UpgradePriceText;
        public TMP_Text DescriptionText;
        public Button PurchaseButton;

        public void SetUpgradeName(string name)
        {
            UpgradeNameText.SetText(name);
        }

        public void SetUpgradePrice(int price)
        {
            UpgradePriceText.SetText(UITextFormatter.FormatPrice(price));
        }

        public void SetUpgradePurchased()
        {
            UpgradePriceText.SetText("Куплено");
            PurchaseButton.interactable = false;
        }

        public void SetUpgradeDescriptionPercent(int percent)
        {
            DescriptionText.SetText(UITextFormatter.FormatUpgradeDescriptionPercent(percent));
        }
    }
}