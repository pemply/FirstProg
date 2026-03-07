using UnityEngine;
using UnityEngine.UI;

public class CritFlash : MonoBehaviour
{
    [SerializeField] private float _flashAlpha = 0.22f;
    [SerializeField] private float _fadeSpeed = 10f;

    private Image _img;
    private float _alpha;

    private void Awake()
    {
        _img = GetComponent<Image>();
        if (_img == null)
        {
            Debug.LogError("[CritFlash] No Image on same GameObject!", this);
            enabled = false;
            return;
        }

        // ✅ щоб НЕ було "завжди жовтий"
        _alpha = 0f;
        SetAlpha(0f);

        // не блокуємо кліки
        _img.raycastTarget = false;

        // гарантовано поверх
        transform.SetAsLastSibling();
    }

    public void Flash()
    {
        if (_img == null) return;

        _alpha = Mathf.Max(_alpha, _flashAlpha);
        SetAlpha(_alpha);
    }

    private void Update()
    {
        if (_alpha <= 0f) return;

        _alpha -= Time.unscaledDeltaTime * _fadeSpeed;
        if (_alpha < 0f) _alpha = 0f;

        SetAlpha(_alpha);
    }

    private void SetAlpha(float a)
    {
        var c = _img.color;
        c.a = a;
        _img.color = c;
    }
}