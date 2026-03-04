using CodeBase.GameLogic;
using CodeBase.GameLogic.Pool;
using CodeBase.Infrastructure.Services.Pool;
using CodeBase.StaticData;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class EnemyAreaAttack : MonoBehaviour, IEnemyAttack
    {
        private AreaAttackConfig _cfg;

        private Transform _hero;
        private IHealth _heroHealth;
        private IPoolService _pool;

        private EnemyAnimator _anim;
        private AgentMoveToPlayer _mover;
        private NavMeshAgent _agent;
        private bool _savedUpdateRotation;

        private float _cd;
        private bool _casting;
        private bool _canAttack;

        private Vector3 _target;
        private GameObject _tele;
        private PooledObject _telePooled;
        private float _turnSpeedDeg = 100f; 
        public bool IsAttacking => _casting;

        public void Construct(Transform hero, IPoolService pool)
        {
            _hero = hero;
            _pool = pool;
            _heroHealth = hero != null ? hero.GetComponentInParent<IHealth>() : null;

            _anim  = GetComponentInParent<EnemyAnimator>(true);
            _mover = GetComponentInParent<AgentMoveToPlayer>(true);
            _agent = GetComponentInParent<NavMeshAgent>(true);
        }

        public void SetConfig(AreaAttackConfig cfg) => _cfg = cfg;

        private void Update()
        {
            if (!_canAttack) return;
            if (_cfg == null) return;
            if (_hero == null || _heroHealth == null) return;

            // ✅ якщо кастує — стоїть, але тулубом повертається до цілі
            if (_casting)
            {
                FaceSmooth(_target);
                return;
            }

            if (_cd > 0f) { _cd -= Time.deltaTime; return; }
            if (!IsHeroInSensor()) return;

            StartCast();
        }


        private bool IsHeroInSensor()
        {
            Vector3 a = transform.position; a.y = 0f;
            Vector3 b = _hero.position;     b.y = 0f;
            return Vector3.Distance(a, b) <= _cfg.SensorRadius;
        }

        private void StartCast()
        {
            _casting = true;
            _cd = _cfg.Cooldown;

            _target = SnapToGround(CalcTargetPoint());
            SpawnTelegraph(_target);

            BeginThrowLock();          // стоп рух
            FaceSmooth(_target);       // одразу почни дивитись у ціль
            _anim?.PlayThrow(_cfg.ThrowSpeed);
        }
       
        private void FaceSmooth(Vector3 worldTarget)
        {
            Vector3 dir = worldTarget - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, _turnSpeedDeg * Time.deltaTime);
        }

        // Animation Event
        public void OnThrowEvent()
        {
            if (!_casting) return;
            if (_pool == null || _cfg == null || _cfg.GrenadePrefab == null)
            {
                FinishCast();
                return;
            }

            Vector3 from = GetThrowOrigin();
            GameObject go = _pool.Get(_cfg.GrenadePrefab, from, Quaternion.identity, null);

            var grenade = go.GetComponent<GrenadeProjectile>();
            if (grenade == null)
            {
                go.GetComponentInParent<PooledObject>(true)?.Release();
                FinishCast();
                return;
            }

            grenade.Throw(from, _target, _cfg.GrenadeFlightTime, _cfg.GrenadeArcHeight, OnGrenadeLanded);
            EndThrowLock(); // після кидка — можна рухатись
        }

        private Vector3 GetThrowOrigin()
        {
            Vector3 from = ThrowSocketPosFallback();

            Vector3 fwd = transform.forward; fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.0001f) fwd.Normalize();

            return from + fwd * _cfg.SpawnForwardOffset + Vector3.up * _cfg.SpawnUpOffset;
        }

        private Vector3 ThrowSocketPosFallback() =>
            transform.position + Vector3.up * 1.2f;

        private void OnGrenadeLanded(Vector3 pos)
        {
            pos = SnapToGround(pos);

            SpawnExplosion(pos);
            ApplyDamage(pos);

            DespawnTelegraph();
            FinishCast();
        }

        private void ApplyDamage(Vector3 pos)
        {
            Vector3 heroPos = _hero.position;
            heroPos.y = pos.y;

            float r = _cfg.AoERadius;
            if ((heroPos - pos).sqrMagnitude <= r * r)
                _heroHealth.TakeDamage(_cfg.Damage);
        }

        private void SpawnTelegraph(Vector3 pos)
        {
            DespawnTelegraph();
            if (_cfg.TelegraphPrefab == null) return;

            _tele = _pool != null
                ? _pool.Get(_cfg.TelegraphPrefab, pos, Quaternion.identity)
                : Instantiate(_cfg.TelegraphPrefab, pos, Quaternion.identity);

            _telePooled = _tele != null ? _tele.GetComponent<PooledObject>() : null;
            _tele?.GetComponent<AoETelegraph>()?.Setup(_cfg.AoERadius);
        }

        private void DespawnTelegraph()
        {
            if (_tele == null) return;

            if (_telePooled != null) _telePooled.Release();
            else Destroy(_tele);

            _tele = null;
            _telePooled = null;
        }

        private void SpawnExplosion(Vector3 pos)
        {
            if (_cfg.ExplosionPrefab == null) return;

            GameObject vfx = _pool != null
                ? _pool.Get(_cfg.ExplosionPrefab, pos, Quaternion.identity)
                : Instantiate(_cfg.ExplosionPrefab, pos, Quaternion.identity);

            if (vfx == null) return;

            // scale під AoERadius
            float baseR = Mathf.Max(0.001f, _cfg.ExplosionVfxBaseRadius);
            float k = _cfg.AoERadius / baseR;

            vfx.transform.localScale = Vector3.one * k;

            // якщо не пул — прибери через lifetime як раніше, якщо треба
        }

        private void FinishCast()
        {
            EndThrowLock();
            _casting = false;
        }

        private Vector3 CalcTargetPoint()
        {
            Vector3 heroPos = _hero.position;
            Vector3 enemyPos = transform.position;

            Vector3 toHero = heroPos - enemyPos; toHero.y = 0f;
            Vector3 forward = toHero.sqrMagnitude > 0.0001f ? toHero.normalized : transform.forward;
            Vector3 right   = Vector3.Cross(Vector3.up, forward).normalized;

            float sideSign = Random.value < 0.5f ? -1f : 1f;

            Vector3 offset = forward * _cfg.ForwardLead + right * (_cfg.SideOffset * sideSign);
            Vector2 j = Random.insideUnitCircle * _cfg.RandomJitter;
            offset += right * j.x + forward * j.y;

            Vector3 target = heroPos + offset;

            Vector3 heroXZ = new Vector3(heroPos.x, 0f, heroPos.z);
            Vector3 enemyXZ = enemyPos; enemyXZ.y = 0f;
            Vector3 targetXZ = target;  targetXZ.y = 0f;

            Vector3 toTarget = targetXZ - enemyXZ;
            float dist = toTarget.magnitude;

            if (dist > _cfg.SensorRadius && dist > 0.0001f)
                targetXZ = enemyXZ + toTarget / dist * _cfg.SensorRadius;

            float minDist = Mathf.Max(0.25f, _cfg.AoERadius * _cfg.MinDistanceFromHeroMult);
            Vector3 heroToTarget = targetXZ - heroXZ;

            if (heroToTarget.magnitude < minDist)
            {
                Vector3 pushDir = (heroXZ - enemyXZ);
                if (pushDir.sqrMagnitude < 0.0001f) pushDir = -forward;
                else pushDir.Normalize();

                targetXZ = heroXZ + pushDir * minDist;
            }

            target.x = targetXZ.x;
            target.z = targetXZ.z;
            target.y = transform.position.y;
            return target;
        }

        private Vector3 SnapToGround(Vector3 p)
        {
            Vector3 origin = p + Vector3.up * 3f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 10f, _cfg.GroundMask, QueryTriggerInteraction.Ignore))
            {
                p.y = hit.point.y;
                return p;
            }

            p.y = _hero != null ? _hero.position.y : p.y;
            return p;
        }

        private void BeginThrowLock()
        {
            if (_mover != null) _mover.enabled = false;

            if (_agent != null && _agent.enabled)
            {
                _savedUpdateRotation = _agent.updateRotation;
                _agent.updateRotation = false;

                if (_agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                    _agent.velocity = Vector3.zero;
                }
            }
        }

        private void EndThrowLock()
        {
            if (_agent != null)
            {
                _agent.updateRotation = _savedUpdateRotation;
                if (_agent.enabled && _agent.isOnNavMesh)
                    _agent.isStopped = false;
            }

            if (_mover != null) _mover.enabled = true;
        }

        public void EnableAttack() => _canAttack = true;

        public void DisableAttack()
        {
            _canAttack = false;
            DespawnTelegraph();
            FinishCast();
        }

        public void ResetForReuse()
        {
            enabled = true;
            _cd = 0f;
            _casting = false;
            _canAttack = false;

            DespawnTelegraph();
            EndThrowLock();
        }
    }
}