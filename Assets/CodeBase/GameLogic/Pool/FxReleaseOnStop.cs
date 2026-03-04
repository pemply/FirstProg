using System.Collections;
using UnityEngine;

namespace CodeBase.GameLogic.Pool
{
    public class FxReleaseWhenDone : MonoBehaviour
    {
        private PooledObject _pooled;
        private ParticleSystem[] _systems;

        [SerializeField] private float _failsafeSeconds = 10f;

        private void OnEnable()
        {
            // refresh every reuse (важливо для пула)
            _pooled = GetComponentInParent<PooledObject>(true);
            _systems = GetComponentsInChildren<ParticleSystem>(true);
            Debug.Log($"[ExplosionFx] enable pooled={_pooled!=null} systems={_systems.Length} root={transform.root.name}", this);

            // перезапуск частинок, щоб не висіли хвости з минулого життя
            for (int i = 0; i < _systems.Length; i++)
            {
                var ps = _systems[i];
                if (ps == null) continue;
                ps.Clear(true);
                ps.Play(true);
            }

            StopAllCoroutines();
            StartCoroutine(WaitForFx());
        }

        private IEnumerator WaitForFx()
        {
            float t = 0f;

            while (AnyAlive() && t < _failsafeSeconds)
            {
                t += Time.deltaTime;
                yield return null;
            }

            // якщо дійшло до failsafe — значить десь looping/stopaction
            if (t >= _failsafeSeconds)
                Debug.LogWarning($"[FxReleaseWhenDone] Failsafe reached on {name}. Check Looping/StopAction.", this);

            _pooled?.Release();
        }

        private bool AnyAlive()
        {
            for (int i = 0; i < _systems.Length; i++)
            {
                var ps = _systems[i];
                if (ps != null && ps.IsAlive(true))
                    return true;
            }
            return false;
        }
    }
}