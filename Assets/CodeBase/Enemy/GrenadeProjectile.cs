using System;
using CodeBase.GameLogic.Pool;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class GrenadeProjectile : MonoBehaviour
    {
        private Vector3 _from, _to;
        private float _time;
        private float _t;
        private float _arc;

        private Action<Vector3> _onLanded;
        private PooledObject _pooled;

        private void Awake()
        {
            _pooled = GetComponentInParent<PooledObject>(true);
        }

        public void Throw(Vector3 from, Vector3 to, float flightTime, float arcHeight, Action<Vector3> onLanded)
        {
            _from = from;
            _to = to;
            _time = Mathf.Max(0.05f, flightTime);
            _arc = arcHeight;
            _t = 0f;
            _onLanded = onLanded;

            transform.position = from;
        }

        private void Update()
        {
            _t += Time.deltaTime / _time;
            float t = Mathf.Clamp01(_t);

            Vector3 p = Vector3.Lerp(_from, _to, t);
            p.y += Mathf.Sin(t * Mathf.PI) * _arc;

            transform.position = p;

            if (t >= 1f)
            {
                _onLanded?.Invoke(_to);
                _onLanded = null;
                _pooled?.Release();
            }
        }
    }
}