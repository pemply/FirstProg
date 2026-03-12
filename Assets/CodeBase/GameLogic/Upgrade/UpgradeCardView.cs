using System;
using System.Collections;
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

        private Coroutine _popRoutine;
        private Coroutine _iconPopRoutine;

        public void SetEmpty()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.interactable = false;
            }

            if (_title != null) _title.text = "—";
            if (_value != null) _value.text = "";

            if (_icon != null)
            {
                _icon.sprite = null;
                _icon.rectTransform.localScale = Vector3.one;
            }

            transform.localScale = Vector3.one;
        }

        public void SetData(
            UpgradeRoll roll,
            UpgradeVisualConfig visuals,
            Action onClick,
            float appearDelay = 0f)
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
            {
                _icon.sprite = visuals != null ? visuals.GetIcon(cfg.Type) : null;
                _icon.rectTransform.localScale = Vector3.one;
            }

            if (_frame != null)
            {
                UpgradeRarity frameRarity = cfg.IgnoreRarity
                    ? UpgradeRarity.Common
                    : roll.Rarity;

                _frame.sprite = visuals != null ? visuals.GetFrame(frameRarity) : null;
            }

            if (_popRoutine != null)
                StopCoroutine(_popRoutine);

            if (_iconPopRoutine != null)
                StopCoroutine(_iconPopRoutine);

            transform.localScale = Vector3.one;

            _popRoutine = StartCoroutine(Pop(appearDelay));
            _iconPopRoutine = StartCoroutine(IconPop(appearDelay + 0.04f));
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

        private IEnumerator Pop(float delay)
        {
            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);

            float duration = 0.25f;
            float t = 0f;

            Vector3 start = Vector3.one * 0.9f;
            Vector3 end = Vector3.one;

            transform.localScale = start;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                float eased = EaseOutBack(k);

                transform.localScale = Vector3.LerpUnclamped(start, end, eased);
                yield return null;
            }

            transform.localScale = end;
            _popRoutine = null;
        }

        private IEnumerator IconPop(float delay)
        {
            if (_icon == null)
                yield break;

            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);

            RectTransform iconRect = _icon.rectTransform;

            float duration = 0.25f;
            float t = 0f;

            Vector3 start = Vector3.one * 0.92f;
            Vector3 end = Vector3.one;

            iconRect.localScale = start;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                float eased = EaseOutBack(k);

                iconRect.localScale = Vector3.LerpUnclamped(start, end, eased);
                yield return null;
            }

            iconRect.localScale = end;
            _iconPopRoutine = null;
        }

        private float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;

            return 1 + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}