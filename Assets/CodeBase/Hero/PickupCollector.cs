using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Hero
{
    public class PickupCollector : MonoBehaviour, ISavedProgressReader, IHeroStatsApplier
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

            // було: pickup.Collect();
            pickup.BeginAttract(transform.root);
        }


        public void LoadProgress(PlayerProgress progress)
        {
            if (progress == null) return;
            ApplyHeroStats(progress.heroStats);
        }

        public void ApplyHeroStats(Stats stats)
        {
            if (stats == null) return;

            if (_trigger == null)
            {
                Debug.LogError("[PickupCollector] _trigger is null in ApplyHeroStats");
                return;
            }

            _trigger.radius = Mathf.Max(0.5f, stats.PickupRadius);
        }
    }
}