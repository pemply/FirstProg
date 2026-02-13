using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class CheckAttackRange : MonoBehaviour
    {
        [SerializeField] private EnemyAttack _attack;
        [SerializeField] private TriggerObserver _triggerObserver;

        private int _playerLayer;

        private void Awake()
        {
            _playerLayer = LayerMask.NameToLayer("Player");

            if (_attack == null)
                _attack = GetComponentInParent<EnemyAttack>(true);

            if (_triggerObserver == null)
                _triggerObserver = GetComponentInChildren<TriggerObserver>(true);
        }

        private void OnEnable()
        {
            if (_triggerObserver == null || _attack == null)
            {
                Debug.LogError("[CheckAttackRange] deps NULL", this);
                enabled = false;
                return;
            }

            _triggerObserver.TriggerEnter += OnEnter;
            _triggerObserver.TriggerStay += OnStay;
            _triggerObserver.TriggerExit += OnExit;

            _attack.DisableAttack();
        }

        private void OnDisable()
        {
            if (_triggerObserver == null) return;

            _triggerObserver.TriggerEnter -= OnEnter;
            _triggerObserver.TriggerStay -= OnStay;
            _triggerObserver.TriggerExit -= OnExit;
        }

        private bool IsHero(Collider c)
        {
            if (c == null) return false;

            // ✅ або сам collider на Player, або root на Player
            if (c.gameObject.layer == _playerLayer) return true;
            if (c.transform.root.gameObject.layer == _playerLayer) return true;

            return false;
        }

        private void OnEnter(Collider other)
        {
            if (!IsHero(other)) return;
            _attack.EnableAttack();
        }

        private void OnStay(Collider other)
        {
            if (!IsHero(other)) return;
            _attack.EnableAttack();
        }

        private void OnExit(Collider other)
        {
            if (!IsHero(other)) return;
            _attack.DisableAttack();
        }
    }
}
