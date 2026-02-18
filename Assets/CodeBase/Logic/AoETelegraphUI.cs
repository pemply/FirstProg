using UnityEngine;
using UnityEngine.UI;

public class AoETelegraphUI : MonoBehaviour
{
    [SerializeField] private Image _fill;
    [SerializeField] private RectTransform _rect;

    private float _duration;
    private float _time;

    public void Setup(float radius, float windup)
    {
        // розмір кола = діаметр
        float size = radius * 2f * 100f; // бо canvas scale 0.01
        _rect.sizeDelta = new Vector2(size, size);

        _duration = windup;
        _time = 0f;

        if (_fill != null)
            _fill.fillAmount = 0f;
    }

    private void Update()
    {
        if (_duration <= 0f) return;

        _time += Time.deltaTime;

        float k = Mathf.Clamp01(_time / _duration);

        if (_fill != null)
            _fill.fillAmount = k;
    }
}