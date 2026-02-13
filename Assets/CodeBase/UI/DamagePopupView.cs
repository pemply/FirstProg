using TMPro;
using UnityEngine;

public class DamagePopupView : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    [Header("Normal")]
    [SerializeField] private float _normalLife = 0.6f;
    [SerializeField] private float _normalFloatSpeed = 60f;

    [Header("Crit")]
    [SerializeField] private float _critLife = 1.0f;
    [SerializeField] private float _critFloatSpeed = 95f;
    [SerializeField] private float _critPopInTime = 0.10f;   // надувся
    [SerializeField] private float _critPopOutTime = 0.18f;  // відскок назад
    [SerializeField] private float _critStartScale = 0.65f;
    [SerializeField] private float _critPeakScale = 1.35f;

    private RectTransform _rt;
    private float _t;
    private float _life;
    private float _floatSpeed;
    private bool _isCrit;
    private Vector3 _baseScale;

    private void Awake()
    {
        _rt = transform as RectTransform;
        if (_text == null) _text = GetComponent<TMP_Text>();

        _baseScale = _rt.localScale;
    }

    public void Show(Vector3 screenPos, int damage, bool isCrit)
    {
        _rt.position = screenPos;
        _isCrit = isCrit;

        if (isCrit)
        {
            _life = _critLife;
            _floatSpeed = _critFloatSpeed;

            // 🔥 CRIT: pop + glitchy shake + золото
            _text.text =
                $"<size=220%><color=#FF2E2E>" +
                $"<incr a=1.25 f=1.8 w=0.55><shake a=2.2 d=0.33 w=0.7>CRIT!</shake></incr>" +
                $"</color></size>\n" +
                $"<size=175%><color=#FFD54A>" +
                $"<incr a=1.18 f=2.2 w=0.70><shake a=1.6 d=0.28 w=0.9>{damage}</shake></incr>" +
                $"</color></size>";

            // стартовий “здутий” scale + далі твій pop-скейл в Update()
            _rt.localScale = _baseScale * _critStartScale;
        }
        else
        {
            _life = _normalLife;
            _floatSpeed = _normalFloatSpeed;

            // ✅ NORMAL: приємний pop + легка хвиля по цифрах (дуже subtle)
            _text.text =
                $"<size=130%><color=#FFFFFF>" +
                $"<incr a=1.08 f=2.6 w=0.35><wave a=0.35 f=1.7 w=0.35>{damage}</wave></incr>" +
                $"</color></size>";

            _rt.localScale = _baseScale;
        }
    }


    private void Update()
    {
        _t += Time.deltaTime;

        // рух вгору
        _rt.position += Vector3.up * _floatSpeed * Time.deltaTime;

        if (_isCrit)
        {
            // ---- POP SCALE: in -> out -> settle ----
            float scaleK;

            if (_t < _critPopInTime)
            {
                float a = _t / Mathf.Max(0.0001f, _critPopInTime);
                // easeOutBack-ish
                scaleK = Mathf.Lerp(_critStartScale, _critPeakScale, EaseOutCubic(a));
            }
            else if (_t < _critPopInTime + _critPopOutTime)
            {
                float a = (_t - _critPopInTime) / Mathf.Max(0.0001f, _critPopOutTime);
                // повертаємось до 1.0
                scaleK = Mathf.Lerp(_critPeakScale, 1.0f, EaseOutCubic(a));
            }
            else
            {
                scaleK = 1.0f;
            }

            _rt.localScale = _baseScale * scaleK;

            // ---- Fade out в кінці ----
            float tail = 0.20f;
            if (_t > _life - tail)
            {
                float a = Mathf.InverseLerp(_life, _life - tail, _t); // 1..0
                var c = _text.color;
                c.a = Mathf.Clamp01(a);
                _text.color = c;
            }
        }

        if (_t >= _life)
            Destroy(gameObject);
    }

    private static float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        float p = 1f - t;
        return 1f - p * p * p;
    }
}
