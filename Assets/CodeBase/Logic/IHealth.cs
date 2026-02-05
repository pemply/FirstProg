using System;

namespace CodeBase.Logic
{
    public interface IHealth
    {
        event Action HealthChanged;
        float maxHealth { get; set; }
        float currentHealth { get; set; }
        void TakeDamage(float damage);
    }
}