using TMPro;
using UnityEngine;

namespace CodeBase.UI
{
    public class CritPopupView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _life = 0.8f;
        [SerializeField] private float _floatSpeed = 80f;

        private RectTransform _rt;
        private float _t;

        private void Awake()
        {
            _rt = transform as RectTransform;
            if (_text == null) _text = GetComponent<TMP_Text>();
        }

        public void Show(Vector3 screenPos, int damage)
        {
            _rt.position = screenPos;

            // СОЧНИЙ CRIT
            _text.text =
                $"<size=160%><color=#FF2E2E><shake>CRIT!</shake></color></size>\n" +
                $"<size=130%><color=#FFD54A><shake>{damage}</shake></color></size>";
        }

        private void Update()
        {
            _t += Time.deltaTime;
            _rt.position += Vector3.up * _floatSpeed * Time.deltaTime;

            if (_t >= _life)
                Destroy(gameObject);
        }
    }
}