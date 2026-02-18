using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class EnemyHealer : MonoBehaviour
    {
        [Header("Heal")]
        [SerializeField] private float _healAmount = 8f;
        [SerializeField] private float _cooldown = 2f;
        [SerializeField] private float _healRadius = 10f;

        [Header("Move / Positioning")]
        [SerializeField] private float _followRadius = 18f;     // в межах чого шукаємо поранених
        [SerializeField] private float _behindTargetDist = 3.5f; // стати "позаду" цілі
        [SerializeField] private float _stopDist = 1.2f;         // коли вважаємо що дійшли

        [Header("FX")]
        [SerializeField] private GameObject _healFxPrefab;
        [SerializeField] private float _fxLifetime = 1f;
        [SerializeField] private Vector3 _fxOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Layers")]
        [SerializeField] private LayerMask _allyMask;

        private Transform _hero;
        private NavMeshAgent _agent;

        private float _cd;
        private float _retargetTimer;

        private IHealth _currentTarget;
        private readonly Collider[] _hits = new Collider[48];

        public void Construct(Transform hero) => _hero = hero;

        public void SetConfig(HealerConfig cfg)
        {
            if (cfg == null) return;

            _cooldown = cfg.Cooldown;
            _healAmount = cfg.HealAmount;
            _healRadius = cfg.HealRadius;

            _behindTargetDist = cfg.BehindDistance;

            _healFxPrefab = cfg.HealFxPrefab;
            _fxLifetime = cfg.FxLifetime;
            _fxOffset = cfg.FxOffset;
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_hero == null) return;

            // 1) ретаргет раз в 0.25с (щоб не дергалось)
            _retargetTimer -= Time.deltaTime;
            if (_retargetTimer <= 0f)
            {
                _currentTarget = FindMostDamagedAlly(ignoreHealers: true);
                _retargetTimer = 0.25f;
            }

            // 2) рух — від цілі
            if (_currentTarget != null)
            {
                Vector3 desiredPos = CalcBehindTargetPoint(_currentTarget);
                MoveTo(desiredPos);
            }
            else
            {
                // fallback: просто тримайся трохи далі від героя (щоб не ліз під меч)
                Vector3 away = (transform.position - _hero.position);
                away.y = 0f;
                if (away.sqrMagnitude < 0.001f) away = Vector3.forward;
                away.Normalize();

                MoveTo(_hero.position + away * 10f);
            }

            // 3) хіл по кулдауну, тільки якщо ціль реально в радіусі хілу
            if (_cd > 0f)
            {
                _cd -= Time.deltaTime;
                return;
            }

            if (_currentTarget == null) return;

            if (!IsInRange(_currentTarget, _healRadius))
                return;

            _currentTarget.Heal(_healAmount);
            PlayHealFx(_currentTarget);

            _cd = _cooldown;
        }

        private IHealth FindMostDamagedAlly(bool ignoreHealers)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, _followRadius, _hits, _allyMask);
            if (count == 0) return null;

            IHealth best = null;
            float bestMissing01 = 0f;

            var self = GetComponentInParent<IHealth>();

            for (int i = 0; i < count; i++)
            {
                var col = _hits[i];
                if (col == null) continue;

                var h = col.GetComponentInParent<IHealth>();
                if (h == null) continue;

                if (ReferenceEquals(h, self))
                    continue;

                // ігноруємо інших хілерів, щоб не збиватись у "кучку хілерів"
                if (ignoreHealers)
                {
                    var mb = h as MonoBehaviour;
                    if (mb != null && mb.GetComponentInChildren<EnemyHealer>() != null)
                        continue;
                }

                if (h is EnemyHealth eh && eh.IsDead) continue;
                if (h.maxHealth <= 0.01f) continue;

                float hp01 = h.currentHealth / h.maxHealth;
                float missing01 = 1f - hp01;

                if (missing01 <= 0.02f) // майже фул — не цікаво
                    continue;

                if (missing01 > bestMissing01)
                {
                    bestMissing01 = missing01;
                    best = h;
                }
            }

            return best;
        }

        private Vector3 CalcBehindTargetPoint(IHealth target)
        {
            var mb = target as MonoBehaviour;
            if (mb == null) return transform.position;

            Vector3 targetPos = mb.transform.position;

            // напрямок "від героя до цілі"
            Vector3 dir = (targetPos - _hero.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) dir = (targetPos - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) dir = Vector3.forward;
            dir.Normalize();

            // "позаду" цілі = далі від героя, за ціллю
            Vector3 desired = targetPos + dir * _behindTargetDist;
            desired.y = transform.position.y;
            return desired;
        }

        private bool IsInRange(IHealth target, float radius)
        {
            var mb = target as MonoBehaviour;
            if (mb == null) return false;

            Vector3 a = transform.position; a.y = 0f;
            Vector3 b = mb.transform.position; b.y = 0f;
            return (a - b).sqrMagnitude <= radius * radius;
        }

        private void MoveTo(Vector3 worldPos)
        {
            if (_agent != null && _agent.enabled)
            {
                _agent.stoppingDistance = _stopDist;
                _agent.isStopped = false;
                _agent.SetDestination(worldPos);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * 2f);
            }
        }

        private void PlayHealFx(IHealth target)
        {
            var mb = target as MonoBehaviour;
            if (mb == null) return;

            if (_healFxPrefab != null)
            {
                var fx = Instantiate(_healFxPrefab, mb.transform.position + _fxOffset, Quaternion.identity);
                fx.transform.SetParent(mb.transform, worldPositionStays: true);
                Destroy(fx, _fxLifetime);
            }

            var flash = mb.GetComponentInChildren<EnemyHealFlash>();
            if (flash != null)
                flash.Play();
        }
    }
}
