using UnityEngine;

namespace CodeBase.Enemy
{
    public class AoETelegraph : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [Header("Intro")]
        [SerializeField] private float _appearTime = 0.18f;

        [Header("Pulse")]
        [SerializeField] private float _pulseSpeed = 6f;
        [SerializeField] private float _minScaleMult = 0.92f;
        [SerializeField] private float _maxScaleMult = 1.05f;

        private float _base;
        private float _t;

        public void Setup(float radius)
        {
            var v = _visual != null ? _visual : transform;

            float diameter = Mathf.Max(0.05f, radius * 2f);
            _base = diameter;

            // старт з нуля (поява)
            v.localScale = Vector3.zero;
            _t = 0f;
        }

        private void Update()
        {
            var v = _visual != null ? _visual : transform;

            // appear
            if (_t < _appearTime)
            {
                _t += Time.deltaTime;
                float k = Mathf.Clamp01(_t / Mathf.Max(0.0001f, _appearTime));
                float s = _base * k;
                v.localScale = new Vector3(s, s, s);
                return;
            }

            // pulse
            float p = 0.5f + 0.5f * Mathf.Sin(Time.time * _pulseSpeed);
            float mult = Mathf.Lerp(_minScaleMult, _maxScaleMult, p);

            float scale = _base * mult;
            v.localScale = new Vector3(scale, scale, scale);
        }
    }
}