using System;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroHealth : MonoBehaviour, ISavedProgressReader, ISavedProgress, IHealth, IHeroStatsApplier
    {
        private Stats _stats;
        private bool _isDead;
        private float _regenHpPerSec;

        public event Action HealthChanged;
        public event Action DeathEvent;

        public float maxHealth
        {
            get => _stats != null ? _stats.MaxHP : 0f;
            set
            {
                if (_stats == null) return;

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
            get => _stats != null ? _stats.CurrentHP : 0f;
            set
            {
                if (_stats == null) return;

                float clamped = Mathf.Clamp(value, 0, _stats.MaxHP);
                if (Math.Abs(_stats.CurrentHP - clamped) > 0.001f)
                {
                    _stats.CurrentHP = clamped;
                    HealthChanged?.Invoke();

                    if (_stats.CurrentHP <= 0 && !_isDead)
                    {
                        _isDead = true;
                        DeathEvent?.Invoke();
                    }
                }
            }
        }

        private void Update()
        {
            if (_isDead) return;

            float perSec = _regenHpPerSec;
            if (perSec <= 0f) return;

            Heal(perSec * Time.deltaTime);
        }

        public void ApplyHeroStats(Stats stats)
        {
            if (stats == null) return;

            _stats = stats;
            _regenHpPerSec = Mathf.Max(0f, stats.RegenHpPerSec);

            if (_stats.MaxHP <= 0) _stats.MaxHP = 1;
            _stats.CurrentHP = Mathf.Clamp(_stats.CurrentHP, 0, _stats.MaxHP);

            _isDead = _stats.CurrentHP <= 0;
            HealthChanged?.Invoke();
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            if (_isDead) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;
            if (_stats == null) { Debug.Log("[HeroHealth] stats null"); return; }
            if (damage <= 0) return;

            currentHealth -= damage;
        }

        public void LoadProgress(PlayerProgress progress)
        {
            progress.heroStats ??= new Stats();
            ApplyHeroStats(progress.heroStats);
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.heroStats ??= new Stats();

            // ✅ MaxHP можна сейвити (бо апгрейди), CurrentHP — краще НЕ сейвити для рана
            progress.heroStats.MaxHP = _stats?.MaxHP ?? progress.heroStats.MaxHP;
        }
    }
}
