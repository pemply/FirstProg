using System;

namespace CodeBase.GameLogic
{
    public interface IHealth
    {
        event Action HealthChanged;
        float maxHealth { get; set; }
        float currentHealth { get; set; }
        void TakeDamage(float damage);
        public void Heal(float amount);
    }
}