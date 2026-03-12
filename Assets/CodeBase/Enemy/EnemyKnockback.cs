using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class EnemyKnockback : MonoBehaviour
    {
        [SerializeField] private float _pushDuration = 0.12f;
        [SerializeField] private float _maxDistance = 10f;

        private NavMeshAgent _agent;

        private Coroutine _routine;
        private bool _cancelled;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void Cancel()
        {
            _cancelled = true;

            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            // не чіпаємо isStopped, якщо агент вже невалидний
        }

        public void Push(Vector3 dir, float power)
        {
            if (_cancelled) return;
            if (power <= 0f) return;

            if (!CanUseAgentForStopResume() && _agent != null && _agent.enabled == false)
                return;

            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(PushRoutine(dir, power));
        }

        private bool CanUseAgentForStopResume()
        {
            return _agent != null
                   && _agent.enabled
                   && _agent.gameObject.activeInHierarchy
                   && _agent.isOnNavMesh;
        }

        private IEnumerator PushRoutine(Vector3 dir, float power)
        {
            if (CanUseAgentForStopResume())
                _agent.isStopped = true;

            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
            {
                if (CanUseAgentForStopResume())
                    _agent.isStopped = false;

                _routine = null;
                yield break;
            }

            dir.Normalize();

            float dist = Mathf.Clamp(power, 0f, _maxDistance);
            Vector3 start = transform.position;
            Vector3 target = start + dir * dist;

            // не даємо штовхати крізь navmesh-перешкоди
            if (NavMesh.Raycast(start, target, out var navHit, NavMesh.AllAreas))
                target = navHit.position;
            else if (NavMesh.SamplePosition(target, out var sampleHit, 1f, NavMesh.AllAreas))
                target = sampleHit.position;

            float t = 0f;
            while (t < _pushDuration)
            {
                if (_cancelled)
                    yield break;

                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / _pushDuration);

                Vector3 p = Vector3.Lerp(start, target, k);

                if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                    _agent.Warp(p);
                else
                    transform.position = p;

                yield return null;
            }

            if (CanUseAgentForStopResume())
                _agent.isStopped = false;

            _routine = null;
        }
    }
}
