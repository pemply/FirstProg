using System.Collections;
using CodeBase.Infrastructure.Services.Progress;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class HeroLevelUI : MonoBehaviour
    {
        private const float PendingPulseSpeed = 3.5f;
        private const float PendingTextScale = 1.08f;

        private const float PendingGlowMinAlpha = 0.05f;
        private const float PendingGlowMaxAlpha = 0.25f;
        private const float PendingGlowSpeed = 5f;

        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Slider _xpBar;
        [SerializeField] private Image _xpFlashOverlay;

        [Header("Bar")]
        [SerializeField] private float _smoothSpeed = 10f;

        [Header("Level Up Flash")]
        [SerializeField] private Color _flashColor = new(1f, 0.96f, 0.8f, 1f);
        [SerializeField] private float _flashInDuration = 0.08f;
        [SerializeField] private float _flashOutDuration = 0.35f;
        [SerializeField] private float _flashMaxAlpha = 0.95f;

        private IXpService _xp;
        private float _targetValue;
        private Coroutine _flashRoutine;
        private Coroutine _levelPopRoutine;
        private Vector3 _baseTextScale;

        public void Construct(IXpService xp)
        {
            _xp = xp;

            if (_levelText != null)
                _baseTextScale = _levelText.rectTransform.localScale;

            if (_xpFlashOverlay != null)
                SetOverlayAlpha(0f);

            if (_xp == null)
                return;

            _xp.Changed += OnXpChanged;
            _xp.LevelUp += OnLevelUp;

            ApplyData(new XpChangedData(_xp.CurrentXpInLevel, _xp.RequiredXp, _xp.Level));
        }

        private void OnDestroy()
        {
            if (_xp == null)
                return;

            _xp.Changed -= OnXpChanged;
            _xp.LevelUp -= OnLevelUp;
        }

        private void Update()
        {
            if (_xpBar != null)
            {
                _xpBar.value = Mathf.Lerp(
                    _xpBar.value,
                    _targetValue,
                    Time.unscaledDeltaTime * _smoothSpeed
                );
            }

            UpdatePendingVisuals();
        }

        private void OnXpChanged(XpChangedData data) => ApplyData(data);

        private void OnLevelUp(int level)
        {
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);

            if (_levelPopRoutine != null)
                StopCoroutine(_levelPopRoutine);

            _flashRoutine = StartCoroutine(PlayFlash());
            _levelPopRoutine = StartCoroutine(PlayLevelPop());
        }

        private void ApplyData(XpChangedData data)
        {
            if (_levelText != null)
                _levelText.text = $"Lv {data.Level}";

            _targetValue = data.Required <= 0
                ? 0f
                : (float)data.Current / data.Required;
        }

        private void UpdatePendingVisuals()
        {
            if (_xp == null)
                return;

            if (_xp.IsLevelUpPending)
            {
                if (_levelText != null && _levelPopRoutine == null)
                {
                    float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * PendingPulseSpeed);
                    float scale = Mathf.Lerp(1f, PendingTextScale, pulse);
                    _levelText.rectTransform.localScale = _baseTextScale * scale;
                }

                if (_xpFlashOverlay != null && _flashRoutine == null)
                {
                    float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * PendingGlowSpeed);
                    float alpha = Mathf.Lerp(PendingGlowMinAlpha, PendingGlowMaxAlpha, pulse);
                    SetOverlayAlpha(alpha);
                }
            }
            else
            {
                if (_levelText != null && _levelPopRoutine == null)
                {
                    _levelText.rectTransform.localScale = Vector3.Lerp(
                        _levelText.rectTransform.localScale,
                        _baseTextScale,
                        Time.unscaledDeltaTime * 10f
                    );
                }

                if (_xpFlashOverlay != null && _flashRoutine == null)
                    SetOverlayAlpha(0f);
            }
        }

        private IEnumerator PlayFlash()
        {
            if (_xpFlashOverlay == null)
                yield break;

            yield return Flash(_flashMaxAlpha, _flashInDuration, _flashOutDuration);
            yield return Flash(_flashMaxAlpha * 0.45f, 0.05f, _flashOutDuration * 1.2f);

            _flashRoutine = null;
        }

        private IEnumerator Flash(float maxAlpha, float inDuration, float outDuration)
        {
            float t = 0f;

            while (t < inDuration)
            {
                t += Time.unscaledDeltaTime;
                SetOverlayAlpha(Mathf.Lerp(0f, maxAlpha, Mathf.Clamp01(t / inDuration)));
                yield return null;
            }

            t = 0f;

            while (t < outDuration)
            {
                t += Time.unscaledDeltaTime;
                SetOverlayAlpha(Mathf.Lerp(maxAlpha, 0f, Mathf.Clamp01(t / outDuration)));
                yield return null;
            }
        }

        private IEnumerator PlayLevelPop()
        {
            if (_levelText == null)
                yield break;

            Vector3 popScale = _baseTextScale * 1.15f;

            float t = 0f;
            const float upTime = 0.08f;
            const float downTime = 0.14f;

            while (t < upTime)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / upTime);
                _levelText.rectTransform.localScale = Vector3.Lerp(_baseTextScale, popScale, k);
                yield return null;
            }

            t = 0f;
            while (t < downTime)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / downTime);
                _levelText.rectTransform.localScale = Vector3.Lerp(popScale, _baseTextScale, k);
                yield return null;
            }

            _levelText.rectTransform.localScale = _baseTextScale;
            _levelPopRoutine = null;
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (_xpFlashOverlay == null)
                return;

            Color c = _flashColor;
            c.a = alpha;
            _xpFlashOverlay.color = c;
        }
    }
}