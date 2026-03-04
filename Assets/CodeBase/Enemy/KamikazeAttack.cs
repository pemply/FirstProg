using System.Collections;
using CodeBase.GameLogic;
using CodeBase.GameLogic.Pool;
using CodeBase.StaticData;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class KamikazeAttack : MonoBehaviour
    {
        // set by factory
        public float EffectiveDistance = 0.5f;   // коли починати ф'юз (ранній тригер)
        public float Damage = 10f;

        // radius of damage + VFX (великий радіус урону)
        public float ExplosionRadius = 1.5f;

        // config
        public float FuseDelay = 0.6f;
        public float BlinkSpeed = 12f;

        // detonation (коли реально підриватись — майже впритул)
        public float DetonatePadding = 0.08f;    // маленький зазор для детонації
        private float _cancelFuseExtra = 0.35f;

        private Transform _heroTransform;
        private IHealth _heroHealth;
        private CharacterController _heroCC;

        private NavMeshAgent _agent;
        private AgentMoveToPlayer _move;
        private EnemyAnimator _anim;

        private Renderer[] _renderers;
        private MaterialPropertyBlock _mpb;

        private bool _arming;
        private Coroutine _routine;

        private PooledObject _pooled;
        private bool _exploded;

        public void Construct(Transform heroTransform)
        {
            _heroTransform = heroTransform;
            _heroHealth = heroTransform != null ? heroTransform.GetComponentInParent<IHealth>() : null;

            if (heroTransform != null)
            {
                _heroCC = heroTransform.GetComponent<CharacterController>();
                if (_heroCC == null)
                    _heroCC = heroTransform.GetComponentInParent<CharacterController>();
            }
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>() ?? GetComponentInParent<NavMeshAgent>();
            _move  = GetComponent<AgentMoveToPlayer>();
            _anim  = GetComponentInParent<EnemyAnimator>() ?? GetComponentInChildren<EnemyAnimator>(true);

            _pooled = GetComponentInParent<PooledObject>(true);

            _renderers = GetComponentsInChildren<Renderer>(true);
            _mpb = new MaterialPropertyBlock();
        }

        public void SetConfig(KamikazeConfig cfg)
        {
            if (cfg == null) return;

            FuseDelay = cfg.FuseDelay;
            BlinkSpeed = cfg.BlinkSpeed;

            // було RadiusPadding -> тепер це padding для детонації (впритул)
            DetonatePadding = cfg.RadiusPadding;
            ExplosionRadius = cfg.ExplosionRadius;
            _cancelFuseExtra = cfg.CancelFuseExtra;
        }

        private void OnDisable()
        {
            StopFuseCoroutine();
        }

        private void Update()
        {
            if (_heroTransform == null || _heroHealth == null || _exploded)
                return;

            float dist = DistanceXZ(transform.position, _heroTransform.position);

            if (_arming)
            {
                Blink();

                if (dist <= DetonateDistance())
                    Explode();

                return;
            }

            // face hero
            Vector3 dir = _heroTransform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);

            // start fuse once
            if (_routine == null && dist <= ArmDistance())
                _routine = StartCoroutine(FuseAndExplode());
        }

        private IEnumerator FuseAndExplode()
        {
            _arming = true;
            StopMove();

            float t = 0f;
            float detonateDist = DetonateDistance();
            float cancelDist   = ArmDistance() + _cancelFuseExtra;

            while (t < FuseDelay)
            {
                if (_heroTransform == null || _heroHealth == null)
                {
                    CancelFuse();
                    yield break;
                }

                float dist = DistanceXZ(transform.position, _heroTransform.position);

                if (dist <= detonateDist)
                    break;

                if (dist > cancelDist)
                {
                    CancelFuse();
                    yield break;
                }

                t += Time.deltaTime;
                yield return null;
            }

            _routine = null;
            Explode();
        }

        private float ArmDistance()
        {
            float heroR  = _heroCC != null ? _heroCC.radius : 0.5f;
            float agentR = _agent != null ? _agent.radius : 0.3f;
            return heroR + agentR + EffectiveDistance;
        }

        // ✅ підриваємось тільки впритул (НЕ плутати з ExplosionRadius)
        private float DetonateDistance()
        {
            float heroR  = _heroCC != null ? _heroCC.radius : 0.5f;
            float agentR = _agent != null ? _agent.radius : 0.3f;
            return heroR + agentR + DetonatePadding;
        }

        private void CancelFuse()
        {
            StopFuseCoroutine();
            _arming = false;

            if (_move != null)
                _move.enabled = true;

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                _agent.isStopped = false;
        }

        private void StopMove()
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
            }

            if (_move != null)
                _move.enabled = false;
        }

        private void StopFuseCoroutine()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        private void Explode()
        {
            if (_exploded) return;
            _exploded = true;

            StopFuseCoroutine();
            _arming = false;

            // damage by ExplosionRadius (великий радіус)
            bool inRange = _heroTransform != null &&
                           DistanceXZ(transform.position, _heroTransform.position) <= ExplosionRadius;

            if (inRange)
                _heroHealth?.TakeDamage(Damage);

            // VFX scale by ExplosionRadius
            _anim?.PlayExplode(ExplosionRadius);

            _pooled?.Release();
        }

        private void Blink()
        {
            float k = 0.5f + 0.5f * Mathf.Sin(Time.time * BlinkSpeed);

            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;

                r.GetPropertyBlock(_mpb);
                _mpb.SetColor("_BaseColor", Color.Lerp(Color.white, Color.red, k));
                _mpb.SetColor("_EmissionColor", Color.red * (0.2f + 2.0f * k));
                r.SetPropertyBlock(_mpb);
            }
        }

        private static float DistanceXZ(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        public void ResetForReuse()
        {
            enabled = true;

            _pooled = GetComponentInParent<PooledObject>(true);

            _arming = false;
            _exploded = false;

            StopFuseCoroutine();

            if (_move != null)
                _move.enabled = true;

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                _agent.isStopped = false;
        }
    }
}