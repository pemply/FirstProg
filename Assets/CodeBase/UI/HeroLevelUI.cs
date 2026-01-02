using CodeBase.Infrastructure.Services.Progress;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class HeroLevelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Slider _xpBar;
        [SerializeField] private float _smoothSpeed = 10f;

        private IXpService _xp;
        private float _targetValue;

        public void Construct(IXpService xp)
        {
            _xp = xp;

            if (_xp != null)
            {
                _xp.Changed += OnXpChanged;

                // первинна синхронізація
                ApplyData(new XpChangedData(
                    _xp.CurrentXpInLevel,
                    _xp.RequiredXp,
                    _xp.Level
                ));
            }
        }

        private void OnDestroy()
        {
            if (_xp != null)
                _xp.Changed -= OnXpChanged;
        }

        private void Update()
        {
            if (_xpBar == null)
                return;

            _xpBar.value = Mathf.Lerp(
                _xpBar.value,
                _targetValue,
                Time.deltaTime * _smoothSpeed
            );
        }

        private void OnXpChanged(XpChangedData data)
        {
            ApplyData(data);
        }

        private void ApplyData(XpChangedData data)
        {
            if (_levelText != null)
                _levelText.text = $"Lv {data.Level}";

            _targetValue = data.Required <= 0
                ? 0
                : (float)data.Current / data.Required;
        }
    }
}