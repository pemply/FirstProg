using System.Collections;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyHealFlash : MonoBehaviour
    {
         private Renderer[] _renderers;
        [SerializeField] private Color _healColor = Color.green;
        [SerializeField] private float _time = 0.15f;

        private Color[] _baseColors;

        private void Awake()
        {
            if (_renderers == null || _renderers.Length == 0)
                _renderers = GetComponentsInChildren<Renderer>();

            _baseColors = new Color[_renderers.Length];

            for (int i = 0; i < _renderers.Length; i++)
                _baseColors[i] = _renderers[i].material.color;
        }

        public void Play()
        {
            StopAllCoroutines();
            StartCoroutine(Flash());
        }

        private IEnumerator Flash()
        {
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].material.color = _healColor;

            yield return new WaitForSeconds(_time);

            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].material.color = _baseColors[i];
        }
    }
}