using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.UI
{
    public class ActorUI : MonoBehaviour
    {
        [SerializeField] private HpBar _hpBar;
        private IHealth _health;

        private void Start()
        {
            IHealth health = GetComponent<IHealth>();
      
            if(health != null)
                Construct(health);
        }
        public void Construct(IHealth health)
        {
            if (_health != null)
                _health.HealthChanged -= UpdateHp;

            _health = health;

            if (_health != null)
                _health.HealthChanged += UpdateHp;

            UpdateHp();
        }

        private void OnDestroy()
        {
            if (_health != null)
                _health.HealthChanged -= UpdateHp;
        }

        private void UpdateHp()
        {
            if (_hpBar == null || _health == null)
                return;

            _hpBar.SetValue(_health.currentHealth, _health.maxHealth);
        }
    }
}