using System;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroHealth : MonoBehaviour, ISavedProgress, IHealth
    {
        private State _state;

        public event Action HealthChanged;
        [SerializeField]
        private float _maxHealth;
        [SerializeField] private float _currentHealth;

     
        public float maxHealth
        {
            get => _state.MaxHP;
            set
            {
                if (Math.Abs(_state.MaxHP - value) > 0.001f)
                {
                    _state.MaxHP = value;
                    if (_state.CurrentHP > _state.MaxHP)
                        _state.CurrentHP = _state.MaxHP;

                    HealthChanged?.Invoke();
                }
            }
        }

        public float currentHealth
        {
            get => _state.CurrentHP;
            set
            {
                float clamped = Mathf.Clamp(value, 0, _state.MaxHP);
                if (Math.Abs(_state.CurrentHP - clamped) > 0.001f)
                {
                    _state.CurrentHP = clamped;
                    HealthChanged?.Invoke();
                }
            }
        }

        public void TakeDamage(float damage)
        {
            Debug.Log($"{name} took {damage}, before={currentHealth}", this);

            currentHealth -= damage;
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _state = progress.HeroState ?? new State(); // ✅ ще один захист
            HealthChanged?.Invoke();
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.HeroState.CurrentHP = currentHealth;
            progress.HeroState.MaxHP = maxHealth;
        }

      

       
    }
}