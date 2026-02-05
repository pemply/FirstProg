using CodeBase.Infrastructure.Services.Progress;
using UnityEngine;

namespace CodeBase.Logic
{
    public class XpPickup : MonoBehaviour
    {
        private int _amount;
        private IXpService _xp;

        private Transform _hero;
        private bool _attract;
        private float _speed;

        [Header("Scatter (optional)")]
        [SerializeField] private float _scatterDuration = 0.15f;
        [SerializeField] private float _scatterSpeed = 4.5f;
        [SerializeField] private float _scatterDrag = 10f;

        [Header("Magnet")]
        [SerializeField] private float _startSpeed = 6f;
        [SerializeField] private float _maxSpeed = 18f;
        [SerializeField] private float _accel = 40f;
        [SerializeField] private float _collectDist = 0.25f;

        private Vector3 _vel;
        private float _scatterLeft;
        private float _collectDistSqr;

        public void Construct(int amount, IXpService xp)
        {
            _amount = amount;
            _xp = xp;

            _hero = null;
            _attract = false;

            _speed = _startSpeed;

            _collectDistSqr = _collectDist * _collectDist;

            // маленький “розліт” одразу після спавну
            _scatterLeft = _scatterDuration;

            // якщо нема scatter — одразу вимикаємо апдейт до моменту BeginAttract()
            enabled = _scatterLeft > 0f;

            if (_scatterLeft > 0f)
            {
                Vector2 rnd = Random.insideUnitCircle;
                if (rnd.sqrMagnitude < 0.0001f)
                    rnd = Vector2.right;

                rnd.Normalize();
                _vel = new Vector3(rnd.x, 0f, rnd.y) * _scatterSpeed;
            }
            else
            {
                _vel = Vector3.zero;
            }
        }

        // Викликає PickupCollector коли пікап зайшов у радіус
        public void BeginAttract(Transform hero)
        {
            if (hero == null) return;

            _hero = hero;
            _attract = true;

            // як тільки герой близько — скидаємо scatter
            _scatterLeft = 0f;

            if (_speed < _startSpeed)
                _speed = _startSpeed;

            enabled = true; // вмикаємо Update тільки коли реально тягнемо/розлітаємось
        }

        // Якщо хочеш зупинку при виході з радіуса — використовуй (можна не викликати)
        public void StopAttract()
        {
            _attract = false;
            _hero = null;
            _speed = _startSpeed;

            // якщо scatter вже закінчився — апдейт не потрібен
            if (_scatterLeft <= 0f)
                enabled = false;
        }

        private void Update()
        {
            // 1) короткий розліт
            if (_scatterLeft > 0f)
            {
                _scatterLeft -= Time.deltaTime;

                transform.position += _vel * Time.deltaTime;
                _vel = Vector3.Lerp(_vel, Vector3.zero, _scatterDrag * Time.deltaTime);

                if (_scatterLeft <= 0f && !_attract)
                    enabled = false; // зупинили Update повністю

                return;
            }

            // 2) магніт тільки коли активували
            if (!_attract || _hero == null)
            {
                enabled = false; // safety: якщо хтось вимкнув attract — гасимо Update
                return;
            }

            Vector3 to = _hero.position - transform.position;
            float sqr = to.sqrMagnitude;

            if (sqr <= _collectDistSqr)
            {
                Collect();
                return;
            }

            _speed = Mathf.Min(_maxSpeed, _speed + _accel * Time.deltaTime);
            transform.position += to.normalized * (_speed * Time.deltaTime);
        }

        public void Collect()
        {
            _xp?.AddXpBuffered(_amount);
            Destroy(gameObject);
        }
    }
}
