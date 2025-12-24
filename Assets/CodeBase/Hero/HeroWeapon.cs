
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroWeapon : MonoBehaviour
    {
        [SerializeField] private TargetSensor _sensor;

        public float Damage = 10f;
        public float Cooldown = 0.8f;
        public float Range = 2.5f;

        private float _cd;

        private void Start()
        {
            _sensor.SetRadius(Range);
        }

        private void Update()
        {
            
            if (_cd > 0)
            {
                _cd -= Time.deltaTime;
                return;
            }

            if (_sensor.TryGetNearest(transform.position, out IHealth target))
            {
                var mb = target as MonoBehaviour;
                Debug.Log($"ATTACK target={(mb ? mb.name : "not-mb")} damage={Damage}", this);
                target.TakeDamage(Damage);
                _cd = Cooldown;
            }
            else
            {
                Debug.Log("ATTACK no target", this);
            }

        }
    }
}
