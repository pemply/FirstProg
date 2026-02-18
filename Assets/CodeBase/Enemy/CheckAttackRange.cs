using UnityEngine;

namespace CodeBase.Enemy
{
    public class CheckAttackRange : MonoBehaviour
    {
        [SerializeField] private TriggerObserver _triggerObserver;

        private IEnemyAttack _attackApi;
        private int _playerLayer;

        private void Awake()
        {
            _playerLayer = LayerMask.NameToLayer("Player");

            // ✅ не беремо "перший MonoBehaviour", беремо саме атаку по інтерфейсу
            _attackApi = GetComponentInParent<IEnemyAttack>(true);

            if (_triggerObserver == null)
                _triggerObserver = GetComponentInChildren<TriggerObserver>(true);
        }

        private void OnEnable()
        {
            if (_triggerObserver == null || _attackApi == null)
            {
                Debug.LogError("[CheckAttackRange] deps NULL or no IEnemyAttack in parents", this);
                enabled = false;
                return;
            }

            _triggerObserver.TriggerEnter += OnEnter;
            _triggerObserver.TriggerStay  += OnStay;
            _triggerObserver.TriggerExit  += OnExit;

            _attackApi.DisableAttack();
        }

        private void OnDisable()
        {
            if (_triggerObserver == null) return;

            _triggerObserver.TriggerEnter -= OnEnter;
            _triggerObserver.TriggerStay  -= OnStay;
            _triggerObserver.TriggerExit  -= OnExit;
        }

        private bool IsHero(Collider c)
        {
            if (c == null) return false;
            if (c.gameObject.layer == _playerLayer) return true;
            if (c.transform.root.gameObject.layer == _playerLayer) return true;
            return false;
        }

        private void OnEnter(Collider other)
        {
            if (!IsHero(other)) return;
            _attackApi.EnableAttack();
        }

        private void OnStay(Collider other)
        {
            if (!IsHero(other)) return;
            _attackApi.EnableAttack(); // лишаємо, щоб не було "пуша"/флуктуацій
        }

        private void OnExit(Collider other)
        {
            if (!IsHero(other)) return;
            _attackApi.DisableAttack();
        }
    }
}
