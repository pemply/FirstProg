using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Hero
{
    public class PickupCollector : MonoBehaviour, ISavedProgressReader, IStatsApplier
    {
        [SerializeField] private SphereCollider _trigger;

        private void Awake()
        {
            if (_trigger == null)
                _trigger = GetComponent<SphereCollider>() ?? GetComponentInChildren<SphereCollider>(true);

            if (_trigger == null)
                Debug.LogError("[PickupCollector] SphereCollider not found");
            else
                _trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root == transform.root)
                return;

            var pickup = other.GetComponentInParent<XpPickup>();
            if (pickup == null)
                return;

            pickup.Collect();
        }

        public void Apply(PlayerProgress progress)
        {
            ApplyStats(progress.heroStats);
        }

        public void ApplyStats(Stats stats)
        {
            if (stats == null) return;

            if (_trigger == null)
            {
                Debug.LogError("[PickupCollector] _trigger is null in ApplyStats");
                return;
            }

            _trigger.radius = Mathf.Max(0.5f, stats.PickupRadius);
            Debug.Log($"[Collector] Applied radius={_trigger.radius}");
        }

        public void LoadProgress(PlayerProgress progress) => Apply(progress);
    }
}