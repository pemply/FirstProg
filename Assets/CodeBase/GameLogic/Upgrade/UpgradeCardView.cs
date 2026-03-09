using System;
using CodeBase.GameLogic.Upgrade;
using CodeBase.StaticData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class UpgradeCardView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _frame;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _value;

        public void SetEmpty()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.interactable = false;
            }

            if (_title != null) _title.text = "—";
            if (_value != null) _value.text = "";
            if (_icon != null) _icon.sprite = null;
        }

        public void SetData(
            UpgradeRoll roll,
            UpgradeVisualConfig visuals,
            Action onClick)
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.interactable = true;
                _button.onClick.AddListener(() => onClick?.Invoke());
            }

            UpgradeConfig cfg = roll.Config;

            if (_title != null)
                _title.text = cfg.GetTitle();

            if (_value != null)
                _value.text = BuildValueText(roll);

            if (_icon != null)
                _icon.sprite = visuals != null ? visuals.GetIcon(cfg.Type) : null;

            if (_frame != null)
            {
                UpgradeRarity frameRarity = cfg.IgnoreRarity
                    ? UpgradeRarity.Common
                    : roll.Rarity;

                _frame.sprite = visuals != null ? visuals.GetFrame(frameRarity) : null;
            }
        }

        private string BuildValueText(UpgradeRoll roll)
        {
            UpgradeConfig cfg = roll.Config;

            if (cfg.Type == UpgradeType.GetSecondaryWeapon)
                return roll.WeaponPreviewId.ToString();

            if (cfg.UsesInt)
                return $"+{roll.IntValue}";

            return $"+{roll.FloatValue:0.##}";
        }
    }
}