using CodeBase.GameLogic.Pool;
using CodeBase.Infrastructure.Services.Progress;
using UnityEngine;

namespace CodeBase.GameLogic
{
    public class XpPickup : MonoBehaviour, IPoolable
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

        // ✅ дані (як у тебе було)
        public void Construct(int amount, IXpService xp)
        {
            _amount = amount;
            _xp = xp;
        }

        // ✅ пул-ресет (гарантовано викликається при Get)
        public void OnSpawned()
        {
            _hero = null;
            _attract = false;

            _speed = _startSpeed;
            _collectDistSqr = _collectDist * _collectDist;

            _scatterLeft = _scatterDuration;

            enabled = _scatterLeft > 0f; // як було

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

        public void OnDespawned()
        {
            // щоб не було “хвостів” при reuse
            _hero = null;
            _attract = false;
            _speed = _startSpeed;
            _vel = Vector3.zero;
            _scatterLeft = 0f;

            enabled = false;

            // щоб не тягнути старі посилання між ранами/сценами
            _xp = null;
            _amount = 0;
        }

        public void BeginAttract(Transform hero)
        {
            if (hero == null) return;

            _hero = hero;
            _attract = true;

            _scatterLeft = 0f;

            if (_speed < _startSpeed)
                _speed = _startSpeed;

            enabled = true;
        }

        public void StopAttract()
        {
            _attract = false;
            _hero = null;
            _speed = _startSpeed;

            if (_scatterLeft <= 0f)
                enabled = false;
        }

        private void Update()
        {
            if (_scatterLeft > 0f)
            {
                _scatterLeft -= Time.deltaTime;

                transform.position += _vel * Time.deltaTime;
                _vel = Vector3.Lerp(_vel, Vector3.zero, _scatterDrag * Time.deltaTime);

                if (_scatterLeft <= 0f && !_attract)
                    enabled = false;

                return;
            }

            if (!_attract || _hero == null)
            {
                enabled = false;
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

            // ✅ пул замість Destroy
            var pooled = GetComponent<PooledObject>();
            if (pooled != null) pooled.Release();
            else Destroy(gameObject);
        }
    }
}