using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyAttack))]
    public class CheckAttackRange : MonoBehaviour
    {
        [SerializeField] private EnemyAttack _attack;
        [SerializeField] private TriggerObserver _triggerObserver;

        private void Awake()
        {
            if (_attack == null)
                _attack = GetComponent<EnemyAttack>();

            if (_triggerObserver == null)
                _triggerObserver = GetComponentInChildren<TriggerObserver>(true);
        }

        private void OnEnable()
        {
            if (_triggerObserver == null)
            {
                Debug.LogError("[CheckAttackRange] TriggerObserver is NULL", this);
                enabled = false;
                return;
            }

            _triggerObserver.TriggerEnter += TriggerEnter;
            _triggerObserver.TriggerStay += TriggerEnter;
            _triggerObserver.TriggerExit += TriggerExit;

            _attack.DisableAttack();
        }

        private void OnDisable()
        {
            if (_triggerObserver == null) return;

            _triggerObserver.TriggerEnter -= TriggerEnter;
            _triggerObserver.TriggerExit -= TriggerExit;
        }

        private void TriggerEnter(Collider obj) => _attack.EnableAttack();
        private void TriggerExit(Collider obj) => _attack.DisableAttack();
    }
}