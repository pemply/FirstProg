using UnityEngine;

namespace CodeBase.Hero
{
    [RequireComponent(typeof(SphereCollider))]
    public class AttackRadiusZoneForHero : MonoBehaviour
    {
        [SerializeField] private SphereCollider _sphere;

        public float Radius => _sphere != null ? _sphere.radius : 0f;

        private void Awake()
        {
            if (_sphere == null)
                _sphere = GetComponent<SphereCollider>();

            _sphere.isTrigger = true;
        }

        public void SetRadius(float radius)
        {
            if (_sphere == null)
                _sphere = GetComponent<SphereCollider>();

            _sphere.radius = radius;
        }
    }
}