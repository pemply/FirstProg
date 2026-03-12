using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class HpBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _damageFlashImage;
        [SerializeField] private Image _heartIcon;
        [SerializeField] private TMP_Text _hpText;

        [Header("Low HP Pulse")]
        [SerializeField] private float _lowHpThreshold = 0.3f;
        [SerializeField] private float _pulseSpeed = 6f;
        [SerializeField] private float _pulseScale = 0.08f;

        [Header("Damage Flash")]
        [SerializeField] private float _flashDelay = 0.08f;
        [SerializeField] private float _flashFadeDuration = 0.35f;

        private float _normalizedHp = 1f;
        private Vector3 _heartBaseScale;
        private Coroutine _flashRoutine;

        private RectTransform _fillRect;
        private RectTransform _flashRect;
        private Color _flashBaseColor;

        private void Awake()
        {
            if (_heartIcon != null)
                _heartBaseScale = _heartIcon.rectTransform.localScale;

            if (_fillImage != null)
                _fillRect = _fillImage.rectTransform;

            if (_damageFlashImage != null)
            {
                _flashRect = _damageFlashImage.rectTransform;
                _flashBaseColor = _damageFlashImage.color;

                Color c = _damageFlashImage.color;
                c.a = 0f;
                _damageFlashImage.color = c;
            }
        }

        private void Update()
        {
            UpdateLowHpPulse();
        }

        public void SetValue(float current, float max)
        {
            if (_fillImage == null || max <= 0f)
                return;

            float oldNormalized = _normalizedHp;
            _normalizedHp = Mathf.Clamp01(current / max);

            _fillImage.fillAmount = _normalizedHp;

            if (_hpText != null)
                _hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

            if (_damageFlashImage != null && _normalizedHp < oldNormalized)
                ShowDamageFlash(oldNormalized, _normalizedHp);
        }

        private void UpdateLowHpPulse()
        {
            if (_heartIcon == null)
                return;

            if (_normalizedHp > _lowHpThreshold)
            {
                _heartIcon.rectTransform.localScale = _heartBaseScale;
                return;
            }

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * _pulseSpeed) * _pulseScale;
            _heartIcon.rectTransform.localScale = _heartBaseScale * pulse;
        }

        private void ShowDamageFlash(float oldValue, float newValue)
        {
            if (_fillRect == null || _flashRect == null || _damageFlashImage == null)
                return;

            float barWidth = _fillRect.rect.width;
            if (barWidth <= 0f)
                return;

            float lostNormalized = oldValue - newValue;
            if (lostNormalized <= 0f)
                return;

            float segmentWidth = barWidth * lostNormalized;
            float segmentStartX = barWidth * newValue;

            _flashRect.anchorMin = new Vector2(0f, 0f);
            _flashRect.anchorMax = new Vector2(0f, 1f);
            _flashRect.pivot = new Vector2(0f, 0.5f);
            _flashRect.anchoredPosition = new Vector2(segmentStartX, 0f);
            _flashRect.sizeDelta = new Vector2(segmentWidth, 0f);

            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);

            _flashRoutine = StartCoroutine(FadeDamageFlash());
        }

        private IEnumerator FadeDamageFlash()
        {
            if (_damageFlashImage == null)
                yield break;

            Color c = _flashBaseColor;
            _damageFlashImage.color = c;

            if (_flashDelay > 0f)
                yield return new WaitForSeconds(_flashDelay);

            float t = 0f;
            while (t < _flashFadeDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / _flashFadeDuration);

                Color fade = _flashBaseColor;
                fade.a = Mathf.Lerp(_flashBaseColor.a, 0f, k);
                _damageFlashImage.color = fade;

                yield return null;
            }

            Color end = _flashBaseColor;
            end.a = 0f;
            _damageFlashImage.color = end;
            _flashRoutine = null;
        }
    }
}