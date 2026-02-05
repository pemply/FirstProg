using UnityEngine;

namespace CodeBase.Hero
{
    [RequireComponent(typeof(SphereCollider))]
    public class AttackRadiusZoneForHero : MonoBehaviour
    {
        [SerializeField] private SphereCollider _sphere;

        private void Awake()
        {
            if (_sphere == null)
                _sphere = GetComponent<SphereCollider>();

            _sphere.isTrigger = true; // тільки щоб не блокував рух, нам тригери не потрібні
        }

        public void SetRadius(float radius)
        {
            if (_sphere == null)
                _sphere = GetComponent<SphereCollider>();

            _sphere.radius = radius;
        }
    }
}