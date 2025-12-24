using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyAttack))]
    public class CheckAttackRange : MonoBehaviour
    {
        [SerializeField] private EnemyAttack _attack;
        [SerializeField] private TriggerObserver _triggerObserver;

        private int _playerLayer;

        private void Awake()
        {
            if (_attack == null)
                _attack = GetComponent<EnemyAttack>();

            if (_triggerObserver == null)
                _triggerObserver = GetComponentInChildren<TriggerObserver>(true);

            _playerLayer = LayerMask.NameToLayer("Player");
        }

        private void OnEnable()
        {
            if (_triggerObserver == null)
            {
                Debug.LogError($"{nameof(CheckAttackRange)}: TriggerObserver is missing!", this);
                enabled = false;
                return;
            }

            _triggerObserver.TriggerEnter += HandleTrigger;
            _triggerObserver.TriggerStay += HandleTrigger;
            _triggerObserver.TriggerExit += OnTriggerExit;

            _attack.DisableAttack();
            _attack.ClearTarget();
        }

        private void OnDisable()
        {
            if (_triggerObserver == null)
                return;

            _triggerObserver.TriggerEnter -= HandleTrigger;
            _triggerObserver.TriggerStay -= HandleTrigger;
            _triggerObserver.TriggerExit -= OnTriggerExit;
        }

        private void HandleTrigger(Collider other)
        {
            // інколи layer стоїть на root, а collider на child — тому root.layer надійніший
            if (other.transform.root.gameObject.layer != _playerLayer)
                return;

            // Якщо вже є валідна ціль — просто тримаємо атаку увімкненою
            // (Stay буде приходити, поки герой всередині)
            if (TryGetHealth(other, out IHealth health))
            {
                _attack.SetTarget(health);
                _attack.EnableAttack();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.root.gameObject.layer != _playerLayer)
                return;

            _attack.ClearTarget();
            _attack.DisableAttack();
        }

        private static bool TryGetHealth(Collider other, out IHealth health)
        {
            if (!other.TryGetComponent(out health))
                health = other.GetComponentInParent<IHealth>();

            return health != null;
        }
    }
}
