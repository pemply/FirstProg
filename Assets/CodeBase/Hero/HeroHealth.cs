using System;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroHealth : MonoBehaviour, ISavedProgress, IHealth
    {
        private Stats _stats;

        public event Action HealthChanged;
        public event Action DeathEvent ;
        [SerializeField]
        private float _maxHealth;
        [SerializeField] private float _currentHealth;
        private bool _isDead;


        public float maxHealth
        {
            get => _stats.MaxHP;
            set
            {
                if (Math.Abs(_stats.MaxHP - value) > 0.001f)
                {
                    _stats.MaxHP = value;
                    if (_stats.CurrentHP > _stats.MaxHP)
                        _stats.CurrentHP = _stats.MaxHP;

                    HealthChanged?.Invoke();
                }
            }
        }

        public float currentHealth
        {
            get => _stats.CurrentHP;
            set
            {
                float clamped = Mathf.Clamp(value, 0, _stats.MaxHP);
                if (Math.Abs(_stats.CurrentHP - clamped) > 0.001f)
                {
                    _stats.CurrentHP = clamped;
                    HealthChanged?.Invoke();
                }
            }
        }

        public void TakeDamage(float damage)
        {
            if (_isDead)
                return;

            currentHealth = currentHealth - damage; // <-- ОЦЕ ГОЛОВНЕ

            if (currentHealth <= 0)
            {
                _isDead = true;
                DeathEvent?.Invoke();
            }
        }


        public void LoadProgress(PlayerProgress progress)
        {
            _stats = progress.heroStats ?? new Stats(); // ✅ ще один захист
            HealthChanged?.Invoke();
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.heroStats ??= new Stats();
            progress.heroStats.CurrentHP = currentHealth;
            progress.heroStats.MaxHP = maxHealth;
        }


        public void ApplyStats(Stats stats)
        {
            _maxHealth = stats.MaxHP;

            // якщо поточне хп більше нового макс — кламп
            if (_currentHealth > _maxHealth)
               _currentHealth = _maxHealth;

            HealthChanged?.Invoke();
        }

    }
}