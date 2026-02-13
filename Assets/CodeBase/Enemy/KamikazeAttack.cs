using System.Collections;
using System.Linq;
using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class KamikazeAttack : MonoBehaviour
    {
        // --- ЦЕ фабрика вже сетить ---
        public float Cleavage = 0.5f;
        public float AttackColdown = 0.5f;
        public float EffectiveDistance = 0.5f;
        public float Damage = 10f;

        // --- НОВЕ: затримка перед вибухом ---
        public float FuseDelay = 0.6f;
        public float RadiusPadding = 0.08f;
        public float BlinkSpeed = 12f;

        private Transform _heroTransform;
        private float _cooldown;
        private bool _arming;
        private int _layerMask;
        private readonly Collider[] _hits = new Collider[1];

        private NavMeshAgent _agent;
        private AgentMoveToPlayer _move; // якщо інша назва - заміниш
        private EnemyAnimator _anim;
        private KamikazeConfig _cfg;
        private Renderer[] _renderers;
        private MaterialPropertyBlock _mpb;

        public void Construct(Transform heroTransform) => _heroTransform = heroTransform;

        private void Awake()
        {
            _layerMask = 1 << LayerMask.NameToLayer("Player");

            _agent = GetComponent<NavMeshAgent>() ?? GetComponentInParent<NavMeshAgent>();
            _move  = GetComponent<AgentMoveToPlayer>();
            _anim  = GetComponentInParent<EnemyAnimator>() ?? GetComponentInChildren<EnemyAnimator>(true);

            _renderers = GetComponentsInChildren<Renderer>(true);
            _mpb = new MaterialPropertyBlock();
        }
        public void SetConfig(KamikazeConfig cfg)
        {
            _cfg = cfg;
            if (_cfg == null) return;

            FuseDelay = _cfg.FuseDelay;
            BlinkSpeed = _cfg.BlinkSpeed;
            RadiusPadding = _cfg.RadiusPadding;
        }
        private void Update()
        {
            if (_heroTransform == null)
                return;

            if (_arming)
            {
                Blink();
                return;
            }

            if (_cooldown > 0f)
            {
                _cooldown -= Time.deltaTime;
                return;
            }

            transform.LookAt(_heroTransform);

            if (!_arming && Hit(out Collider hit))
            {
                _arming = true;
                StartCoroutine(FuseAndExplode(hit));
            }

        }

        private IEnumerator FuseAndExplode(Collider hit)
        {
            _arming = true;

            // стоп щоб став на місці
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
            }

            if (_move != null)
                _move.enabled = false;

            float t = 0f;
            while (t < FuseDelay)
            {
                t += Time.deltaTime;
                yield return null;
            }
            hit.GetComponentInParent<IHealth>()?.TakeDamage(Damage);
            _anim?.PlayExplode();

            enabled = false;
        }


        private bool Hit(out Collider hit)
        {
            hit = null;

            float heroR = _heroTransform.GetComponent<CharacterController>()?.radius ?? 0.5f;
            float agentR = _agent != null ? _agent.radius : 0.3f;

            float explodeRadius = heroR + agentR + RadiusPadding;

            Vector3 p = transform.position + Vector3.up * 0.5f;
            int hitCount = Physics.OverlapSphereNonAlloc(p, explodeRadius, _hits, _layerMask);

            if (hitCount > 0)
            {
                hit = _hits[0];
                return true;
            }

            return false;
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
    }
}
