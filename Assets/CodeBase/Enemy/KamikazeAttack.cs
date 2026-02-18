using System.Collections;
using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class KamikazeAttack : MonoBehaviour
    {
        // set by factory
        public float AttackColdown = 0.5f;
        public float EffectiveDistance = 0.5f; // ✅ “додаткова” дистанція до героя (після врахування радіусів)
        public float Damage = 10f;

        // config
        public float FuseDelay = 0.6f;
        public float RadiusPadding = 0.08f;
        public float BlinkSpeed = 12f;

        private Transform _heroTransform;
        private IHealth _heroHealth;
        private CharacterController _heroCC;

        private NavMeshAgent _agent;
        private AgentMoveToPlayer _move;
        private EnemyAnimator _anim;

        private Renderer[] _renderers;
        private MaterialPropertyBlock _mpb;

        private float _cooldown;
        private bool _arming;
        private Coroutine _routine;

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

            _renderers = GetComponentsInChildren<Renderer>(true);
            _mpb = new MaterialPropertyBlock();
        }

        public void SetConfig(KamikazeConfig cfg)
        {
            if (cfg == null) return;
            FuseDelay = cfg.FuseDelay;
            BlinkSpeed = cfg.BlinkSpeed;
            RadiusPadding = cfg.RadiusPadding;
        }

        private void OnDisable()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        private void Update()
        {
            if (_heroTransform == null || _heroHealth == null)
                return;

            if (_arming)
            {
                Blink();

                // якщо вже “впритул” — вибух одразу
                if (DistanceXZ(transform.position, _heroTransform.position) <= ExplodeDistance())
                    Explode();

                return;
            }

            if (_cooldown > 0f)
            {
                _cooldown -= Time.deltaTime;
                return;
            }

            // поворот (горизонтально)
            Vector3 dir = _heroTransform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);

            // ✅ старт fuse на “реальній” дистанції: радіуси + EffectiveDistance
            float dist = DistanceXZ(transform.position, _heroTransform.position);
            if (dist <= ArmDistance())
            {
                _routine = StartCoroutine(FuseAndExplode());
            }
        }

        private IEnumerator FuseAndExplode()
        {
            _arming = true;

            // стопаємося тільки коли вже армимося (так воно не буде “оббігати”)
            StopMove();

            float t = 0f;
            float explodeDist = ExplodeDistance();

            while (t < FuseDelay)
            {
                t += Time.deltaTime;

                // якщо вже “впритул” — бахаємо раніше
                if (DistanceXZ(transform.position, _heroTransform.position) <= explodeDist)
                    break;

                yield return null;
            }

            Explode();
        }

        private float ArmDistance()
        {
            float heroR  = _heroCC != null ? _heroCC.radius : 0.5f;
            float agentR = _agent != null ? _agent.radius : 0.3f;

            // ✅ EffectiveDistance тепер реально працює як “додаткова” дистанція до героя
            return heroR + agentR + EffectiveDistance;
        }

        private float ExplodeDistance()
        {
            float heroR  = _heroCC != null ? _heroCC.radius : 0.5f;
            float agentR = _agent != null ? _agent.radius : 0.3f;
            return heroR + agentR + RadiusPadding;
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

        private void Explode()
        {
            if (!enabled) return;

            _heroHealth?.TakeDamage(Damage);
            _anim?.PlayExplode();

            _cooldown = AttackColdown;
            enabled = false;
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
    }
}
