using System;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroHealth : MonoBehaviour, ISavedProgress, IHealth, IStatsApplier, IHeroStatsApplier
    {
        private Stats _stats;
        private bool _isDead;

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
        public void ApplyHeroStats(Stats stats)
        {
            if (stats == null) return;

            // важливо: ми працюємо з тим самим об'єктом Stats, що в прогресі
            _stats = stats;

            if (_stats.MaxHP <= 0) _stats.MaxHP = 1;
            _stats.CurrentHP = Mathf.Clamp(_stats.CurrentHP, 0, _stats.MaxHP);

            _isDead = _stats.CurrentHP <= 0;

            HealthChanged?.Invoke();
        }

        public void Apply(PlayerProgress progress)
        {
            // замість дублю логіки — викликаємо ApplyHeroStats
            progress.heroStats ??= new Stats();
            ApplyHeroStats(progress.heroStats);
        }
        public void TakeDamage(float damage)
        {
            if (_isDead) return;
            if (_stats == null) { Debug.Log("[HeroHealth] stats null"); return; }
            if (damage <= 0) return;

          
            currentHealth -= damage;
        }


        public void LoadProgress(PlayerProgress progress) => Apply(progress);

        public void UpdateProgress(PlayerProgress progress)
        {
            // якщо  сейвиш між сесіями — ок.
            // якщо сейв не потрібен — можна прибрати ISavedProgress взагалі.
            progress.heroStats ??= new Stats();
            progress.heroStats.MaxHP = _stats?.MaxHP ?? progress.heroStats.MaxHP;
            progress.heroStats.CurrentHP = _stats?.CurrentHP ?? progress.heroStats.CurrentHP;
        }

    
    }
}
