using CodeBase.GameLogic.Pool;
using TMPro;
using UnityEngine;

public class DamagePopupView : MonoBehaviour, IPoolable
{
    [SerializeField] private TMP_Text _text;

    [Header("Normal")]
    [SerializeField] private float _normalLife = 0.6f;
    [SerializeField] private float _normalFloatSpeed = 60f;

    [Header("Crit")]
    [SerializeField] private float _critLife = 1.0f;
    [SerializeField] private float _critFloatSpeed = 95f;
    [SerializeField] private float _critPopInTime = 0.10f;
    [SerializeField] private float _critPopOutTime = 0.18f;
    [SerializeField] private float _critStartScale = 0.65f;
    [SerializeField] private float _critPeakScale = 1.35f;

    private RectTransform _rt;
    private float _t;
    private float _life;
    private float _floatSpeed;
    private bool _isCrit;
    private Vector3 _baseScale;

    private PooledObject _pooled;

    // 🔥 важливо: поки не Show() — Update нічого не робить
    private bool _armed;

    private void Awake()
    {
        _rt = transform as RectTransform;
        if (_text == null) _text = GetComponent<TMP_Text>();
        _baseScale = (_rt != null ? _rt.localScale : Vector3.one) * 0.4f;    }

    private void EnsurePooled()
    {
        if (_pooled == null)
            _pooled = GetComponentInParent<PooledObject>();
    }

    public void Show(Vector3 screenPos, int damage, bool isCrit)
    {
        EnsurePooled();

        if (_rt == null) _rt = transform as RectTransform;
        _rt.position = screenPos;

        _isCrit = isCrit;
        _t = 0f;

        // 🔥 ставимо life/speed ДО того, як armed стане true
        if (isCrit)
        {
            _life = _critLife;
            _floatSpeed = _critFloatSpeed;

            _text.text =
                $"<size=220%><color=#FF2E2E>" +
                $"<incr a=1.25 f=1.8 w=0.55><shake a=2.2 d=0.33 w=0.7>CRIT!</shake></incr>" +
                $"</color></size>\n" +
                $"<size=175%><color=#FFD54A>" +
                $"<incr a=1.18 f=2.2 w=0.70><shake a=1.6 d=0.28 w=0.9>{damage}</shake></incr>" +
                $"</color></size>";

            _rt.localScale = _baseScale * _critStartScale;
        }
        else
        {
            _life = _normalLife;
            _floatSpeed = _normalFloatSpeed;

            _text.text =
                $"<size=130%><color=#FFFFFF>" +
                $"<incr a=1.08 f=2.6 w=0.35><wave a=0.35 f=1.7 w=0.35>{damage}</wave></incr>" +
                $"</color></size>";

            _rt.localScale = _baseScale;
        }

        ResetVisualState();

        // 🔥 тепер можна апдейтитись
        _armed = true;
    }

    private void Update()
    {
        if (!_armed)
            return;

        _t += Time.deltaTime;

        MoveUp();
        ApplyCritScale();   // тільки якщо _isCrit
        ApplyTailFade();    // для всіх

        if (_t >= _life)
            Despawn();
    }

    private void MoveUp()
    {
        if (_rt == null) return;
        _rt.position += Vector3.up * _floatSpeed * Time.deltaTime;
    }

    private void ApplyCritScale()
    {
        if (!_isCrit || _rt == null) 
            return;

        float scaleK;

        if (_t < _critPopInTime)
        {
            float a = _t / Mathf.Max(0.0001f, _critPopInTime);
            scaleK = Mathf.Lerp(_critStartScale, _critPeakScale, EaseOutCubic(a));
        }
        else if (_t < _critPopInTime + _critPopOutTime)
        {
            float a = (_t - _critPopInTime) / Mathf.Max(0.0001f, _critPopOutTime);
            scaleK = Mathf.Lerp(_critPeakScale, 1.0f, EaseOutCubic(a));
        }
        else
        {
            scaleK = 1.0f;
        }

        _rt.localScale = _baseScale * scaleK;
    }

    private void ApplyTailFade()
    {
        if (_text == null) return;

        float tail = 0.18f;                 // 0.18–0.22 норм
        if (_t <= _life - tail) return;

        float a = Mathf.InverseLerp(_life, _life - tail, _t);

        var c = _text.color;
        c.a = Mathf.Clamp01(a);
        _text.color = c;
    }

    private void Despawn()
    {
        _armed = false;

        EnsurePooled();
        if (_pooled != null) _pooled.Release();
        else Destroy(gameObject);
    }

    public void OnSpawned()
    {
        EnsurePooled();

        _armed = false;   // 🔥 поки Show не викликали — не апдейтимось
        _t = 0f;

        // поставимо дефолт, щоб не було life=0
        _life = _normalLife;
        _floatSpeed = _normalFloatSpeed;

        ResetVisualState();
    }

    public void OnDespawned()
    {
        _armed = false;
        _t = 0f;
    }

    private void ResetVisualState()
    {
        if (_text != null)
        {
            var c = _text.color;
            c.a = 1f;
            _text.color = c;
        }

        if (!_isCrit && _rt != null)
            _rt.localScale = _baseScale;
    }
    public void ShowHealLike(Vector3 screenPos, int heal, float life, float floatSpeed)
    {
        EnsurePooled();

        if (_rt == null) _rt = transform as RectTransform;
        _rt.position = screenPos;

        _isCrit = false;
        _t = 0f;

        _life = Mathf.Max(0.15f, life);
        _floatSpeed = Mathf.Max(10f, floatSpeed);

        // ✅ більш “солодкий” heal: +, зелений, легкий shake/wave
        _text.text =
            $"<size=155%><color=#49FF79>" +
            $"<incr a=1.14 f=2.0 w=0.60>" +
            $"<wave a=0.18 f=2.8 w=0.60>+{heal}</wave>" +
            $"</incr></color></size>";

        // ✅ легкий pop-in (як “пружинка”)
        _rt.localScale = _baseScale * 0.88f;

        ResetVisualState();
        _armed = true;
    }
    private static float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        float p = 1f - t;
        return 1f - p * p * p;
    }
}