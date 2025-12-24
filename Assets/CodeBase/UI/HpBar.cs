using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class HpBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;

  
        public void SetValue(float current, float max)
        {
            Debug.Log($"[HpBar] current={current} max={max} fillImg={_fillImage}", this);

            if (_fillImage == null || max <= 0)
                return;

            _fillImage.fillAmount = Mathf.Clamp01(current / max);
        }

    }
}