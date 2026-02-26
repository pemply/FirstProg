using System.Collections;
using CodeBase.GameLogic;
using CodeBase.GameLogic.Pool;
using CodeBase.Infrastructure.Services.Pool;
using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyAreaAttack : MonoBehaviour, IEnemyAttack
    {
        [Header("Damage")]
        public float Damage = 10f;

        [Header("Timing")]
        public float Cooldown = 2.5f;
        public float Windup = 0.9f;

        [Header("AOE")]
        public float AoERadius = 1.8f;
        public GameObject TelegraphPrefab;

        [Header("Range")]
        public float SensorRadius = 8f;

        private float _forwardLead = 1.5f;
        private float _sideOffset = 1f;
        private float _randomJitter = 0.5f;
        private float _minDistanceFromHeroMult = 0.8f;

        private Transform _hero;
        private IHealth _heroHealth;

        private float _cd;
        private bool _casting;
        private bool _canAttack;

        private Coroutine _castRoutine;

        // ✅ pool
        private IPoolService _pool;
        private GameObject _telegraphInstance;
        private PooledObject _telegraphPooled;

        public bool IsAttacking => _casting;

        public void Construct(Transform hero, IPoolService pool)
        {
            _hero = hero;
            _pool = pool;
            _heroHealth = hero != null ? hero.GetComponentInParent<IHealth>() : null;
        }

        public void SetConfig(AreaAttackConfig cfg)
        {
            if (cfg == null) return;

            Damage = cfg.Damage;
            Cooldown = cfg.Cooldown;
            Windup = cfg.Windup;
            AoERadius = cfg.AoERadius;
            TelegraphPrefab = cfg.TelegraphPrefab;
            SensorRadius = cfg.SensorRadius;

            _forwardLead = cfg.ForwardLead;
            _sideOffset = cfg.SideOffset;
            _randomJitter = cfg.RandomJitter;
            _minDistanceFromHeroMult = cfg.MinDistanceFromHeroMult;
        }

        private void Update()
        {
            if (!_canAttack) return;
            if (_casting) return;
            if (_hero == null || _heroHealth == null) return;

            if (_cd > 0f)
            {
                _cd -= Time.deltaTime;
                return;
            }

            Vector3 a = transform.position; a.y = 0f;
            Vector3 b = _hero.position;     b.y = 0f;
            float dist = Vector3.Distance(a, b);

            if (dist > SensorRadius)
                return;

            _castRoutine = StartCoroutine(CastAtOffsetTarget());
        }

        private IEnumerator CastAtOffsetTarget()
        {
            _casting = true;
            _cd = Cooldown;

            Vector3 target = CalcTargetPoint();
            target.y = transform.position.y;

            SpawnTelegraph(target);

            try
            {
                float time = 0f;
                while (time < Windup)
                {
                    time += Time.deltaTime;
                    yield return null;
                }

                if (_hero != null && _heroHealth != null)
                {
                    Vector3 heroPos = _hero.position;
                    heroPos.y = target.y;

                    if ((heroPos - target).sqrMagnitude <= AoERadius * AoERadius)
                        _heroHealth.TakeDamage(Damage);
                }
            }
            finally
            {
                DespawnTelegraph();   // ✅ гарантовано
                _casting = false;
                _castRoutine = null;
            }
        }

        private Vector3 CalcTargetPoint()
        {
            Vector3 heroPos = _hero.position;
            Vector3 heroXZ = new Vector3(heroPos.x, 0f, heroPos.z);

            Vector3 forward = _hero.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;
            else
                forward.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            float sideSign = Random.value < 0.5f ? -1f : 1f;

            Vector3 offset = forward * _forwardLead + right * (_sideOffset * sideSign);

            Vector2 j = Random.insideUnitCircle * _randomJitter;
            offset += right * j.x + forward * j.y;

            Vector3 target = heroPos + offset;

            Vector3 enemyXZ = transform.position; enemyXZ.y = 0f;
            Vector3 targetXZ = target; targetXZ.y = 0f;

            Vector3 toTarget = targetXZ - enemyXZ;
            float dist = toTarget.magnitude;

            if (dist > SensorRadius && dist > 0.0001f)
                targetXZ = enemyXZ + toTarget / dist * SensorRadius;

            float minDist = Mathf.Max(0.25f, AoERadius * _minDistanceFromHeroMult);
            Vector3 heroToTarget = targetXZ - heroXZ;

            if (heroToTarget.magnitude < minDist)
            {
                Vector3 pushDir = (heroXZ - enemyXZ);
                if (pushDir.sqrMagnitude < 0.0001f)
                    pushDir = -forward;
                else
                    pushDir.Normalize();

                targetXZ = heroXZ + pushDir * minDist;
            }

            target.x = targetXZ.x;
            target.z = targetXZ.z;
            return target;
        }

        private void SpawnTelegraph(Vector3 target)
        {
            if (TelegraphPrefab == null) return;

            DespawnTelegraph();

            // ✅ якщо пула ще нема — fallback на Instantiate
            _telegraphInstance = _pool != null
                ? _pool.Get(TelegraphPrefab, target, Quaternion.identity)
                : Instantiate(TelegraphPrefab, target, Quaternion.identity);

            _telegraphPooled = _telegraphInstance != null
                ? _telegraphInstance.GetComponent<PooledObject>()
                : null;

            var tele = _telegraphInstance != null ? _telegraphInstance.GetComponent<AoETelegraph>() : null;
            if (tele != null)
                tele.Setup(AoERadius);
        }

        private void DespawnTelegraph()
        {
            if (_telegraphInstance == null) return;

            if (_telegraphPooled != null)
                _telegraphPooled.Release();
            else
                Destroy(_telegraphInstance);

            _telegraphInstance = null;
            _telegraphPooled = null;
        }

        public void EnableAttack() => _canAttack = true;

        public void DisableAttack()
        {
            _canAttack = false;

            if (_castRoutine != null)
            {
                StopCoroutine(_castRoutine);
                _castRoutine = null;
            }

            DespawnTelegraph();
            _casting = false;
        }
        public void ResetForReuse()
        {
            enabled = true;

            _cd = 0f;
            _casting = false;
            _canAttack = false;

            if (_castRoutine != null)
            {
                StopCoroutine(_castRoutine);
                _castRoutine = null;
            }

            DespawnTelegraph();
        }
    }
}