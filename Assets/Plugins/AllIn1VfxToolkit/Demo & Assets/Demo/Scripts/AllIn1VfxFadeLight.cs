using UnityEngine;

namespace AllIn1VfxToolkit.Demo.Scripts
{
    [RequireComponent(typeof(Light))]
    public class AllIn1VfxFadeLight : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.1f;

        private Light _targetLight;
        private float _animationRatioRemaining;
        private float _initialLightIntensity;

        private void Awake()
        {
            _targetLight = GetComponent<Light>();
            _initialLightIntensity = _targetLight.intensity;
        }

        private void OnEnable()
        {
            if (_targetLight == null)
                _targetLight = GetComponent<Light>();

            _animationRatioRemaining = 1f;
            _targetLight.intensity = _initialLightIntensity;
        }

        private void Update()
        {
            if (_targetLight == null)
                return;

            _targetLight.intensity = Mathf.Lerp(0f, _initialLightIntensity, _animationRatioRemaining);
            _animationRatioRemaining -= Time.deltaTime / fadeDuration;

            if (_animationRatioRemaining < 0f)
                _animationRatioRemaining = 0f;
        }
    }
}