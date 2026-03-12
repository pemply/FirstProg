using System.Collections;
using UnityEngine;

namespace CodeBase.GameLogic.Pool
{
    public class FxReleaseWhenDone : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 0.5f;

        private PooledObject _pooled;
        private ParticleSystem[] _systems;

        private void OnEnable()
        {
            _pooled = GetComponentInParent<PooledObject>(true);
            _systems = GetComponentsInChildren<ParticleSystem>(true);

            for (int i = 0; i < _systems.Length; i++)
            {
                var ps = _systems[i];
                if (ps == null) 
                    continue;

                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Clear(true);
                ps.Play(true);
            }

            StopAllCoroutines();
            StartCoroutine(ReleaseAfterTime());
        }

        private IEnumerator ReleaseAfterTime()
        {
            yield return new WaitForSeconds(_lifeTime);
            _pooled?.Release();
        }
    }
}