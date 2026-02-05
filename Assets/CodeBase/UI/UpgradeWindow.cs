using System;
using CodeBase.Logic.Upgrade;
using CodeBase.StaticData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class UpgradeWindow : MonoBehaviour
    {
        [SerializeField] private Button _btn1;
        [SerializeField] private Button _btn2;
        [SerializeField] private Button _btn3;

        [SerializeField] private TMP_Text _txt1;
        [SerializeField] private TMP_Text _txt2;
        [SerializeField] private TMP_Text _txt3;

        private Action<int> _onPick;

        public void Show(UpgradeRoll[] choices, Action<int> onPick)
        {
            _onPick = onPick;
            gameObject.SetActive(true);

            SetButton(_btn1, _txt1, choices, 0);
            SetButton(_btn2, _txt2, choices, 1);
            SetButton(_btn3, _txt3, choices, 2);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onPick = null;
        }

        private void SetButton(Button btn, TMP_Text txt, UpgradeRoll[] choices, int index)
        {
            btn.onClick.RemoveAllListeners();

            var roll = (choices != null && index < choices.Length) ? choices[index] : default;

            if (roll.Config == null)
            {
                btn.interactable = false;
                if (txt != null) txt.text = "—";
                return;
            }

            btn.interactable = true;

            if (txt != null)
                txt.text = BuildText(roll);

            btn.onClick.AddListener(() => _onPick?.Invoke(index));
        }

        private string BuildText(UpgradeRoll roll)
        {
            UpgradeConfig cfg = roll.Config;

            string rarity = cfg.IgnoreRarity ? "" : $" [{roll.Rarity}]";

            if (cfg.Type == UpgradeType.GetSecondaryWeapon)
                return $"{cfg.GetTitle()}{rarity} -> {roll.WeaponPreviewId}";

            if (cfg.UsesInt)
                return $"{cfg.GetTitle()}{rarity} +{roll.IntValue}";

            return $"{cfg.GetTitle()}{rarity} +{roll.FloatValue:0.##}";
        }



    }
}
