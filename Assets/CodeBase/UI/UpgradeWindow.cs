using System;
using CodeBase.GameLogic.Upgrade;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.UI
{
    public class UpgradeWindow : MonoBehaviour
    {
        [SerializeField] private UpgradeCardView _card1;
        [SerializeField] private UpgradeCardView _card2;
        [SerializeField] private UpgradeCardView _card3;
        [SerializeField] private UpgradeVisualConfig _visuals;

        private Action<int> _onPick;

        public void Show(UpgradeRoll[] choices, Action<int> onPick)
        {
            _onPick = onPick;
            gameObject.SetActive(true);

            SetCard(_card1, choices, 0, 0f);
            SetCard(_card2, choices, 1, 0.1f);
            SetCard(_card3, choices, 2, 0.2f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onPick = null;
        }

        private void SetCard(UpgradeCardView card, UpgradeRoll[] choices, int index, float appearDelay)
        {
            if (card == null)
                return;

            var roll = (choices != null && index < choices.Length) ? choices[index] : default;

            if (roll.Config == null)
            {
                card.SetEmpty();
                return;
            }

            card.SetData(roll, _visuals, () => _onPick?.Invoke(index), appearDelay);
        }
    }
}