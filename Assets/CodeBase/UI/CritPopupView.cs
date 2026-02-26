using CodeBase.GameLogic.Pool;
using TMPro;
using UnityEngine;
using CodeBase.Infrastructure.Services.Pool;

namespace CodeBase.UI
{
    public class CritPopupView : MonoBehaviour, IPoolable
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _life = 0.8f;
        [SerializeField] private float _floatSpeed = 80f;

        private RectTransform _rt;
        private float _t;
        private PooledObject _pooled;

        private void Awake()
        {
            _rt = transform as RectTransform;
            if (_text == null) _text = GetComponent<TMP_Text>();

            _pooled = GetComponent<PooledObject>(); // якщо нема — пул сам додасть при CreateNew()
        }

        public void Show(Vector3 screenPos, int damage)
        {
            _t = 0f;                 // <-- важливо
            _rt.position = screenPos;

            // СОЧНИЙ CRIT (НЕ ЧІПАЮ)
            _text.text =
                $"<size=160%><color=#FF2E2E><shake>CRIT!</shake></color></size>\n" +
                $"<size=130%><color=#FFD54A><shake>{damage}</shake></color></size>";

            // якщо раніше могла мінятись альфа
            var c = _text.color; c.a = 1f; _text.color = c;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            _rt.position += Vector3.up * _floatSpeed * Time.deltaTime;

            if (_t >= _life)
            {
                if (_pooled != null) _pooled.Release();
                else Destroy(gameObject); // fallback якщо запустив без пулу
            }
        }

        // --- IPoolable ---
        public void OnSpawned()
        {
            _t = 0f;
            var c = _text.color; c.a = 1f; _text.color = c;
        }

        public void OnDespawned()
        {
            _t = 0f;
        }
    }
}