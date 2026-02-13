using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class PersistentAuraFx
    {
        private System.Func<Transform> _parentGetter;

        private WeaponConfig _cfg;
        private WeaponStats _stats;

        private GameObject _instance;
        private bool _hasStats;

        // NEW: базовий scale інстанса (як у префаба)
        private Vector3 _baseLocalScale = Vector3.one;
        private bool _hasBaseScale;

        public void SetParentGetter(System.Func<Transform> parentGetter)
            => _parentGetter = parentGetter;

        public void SetConfig(WeaponConfig cfg)
        {
            if (_cfg == cfg) return;

            _cfg = cfg;
            Debug.Log($"[AuraFx] SetConfig cfg={_cfg?.name} shape={_cfg?.BaseStats.Shape} fxPrefab={_cfg?.PersistentFxPrefab}");

            Clear();
        }

        public void ApplyStats(WeaponStats stats)
        {
            _stats = stats;
            _hasStats = true;
            Ensure();
            UpdateScale();
        }

        public void OnEnable()
        {
            if (_hasStats)
            {
                Ensure();
                UpdateScale();
            }
        }

        public void OnDisable() { }

        private void Ensure()
        {
            if (_instance != null) return;

            if (_cfg == null) { Debug.Log("[AuraFx] Ensure: cfg is null"); return; }
            if (_cfg.BaseStats.Shape != WeaponStats.AttackShape.Aura) { Debug.Log($"[AuraFx] Ensure: shape is {_cfg.BaseStats.Shape}, not Aura"); return; }
            if (_cfg.PersistentFxPrefab == null) { Debug.Log("[AuraFx] Ensure: fxPrefab is null"); return; }
            if (!_hasStats) { Debug.Log("[AuraFx] Ensure: no stats yet"); return; }

            Transform parent = _parentGetter != null ? _parentGetter() : null;
            if (parent == null) { Debug.Log("[AuraFx] Ensure: parent is null"); return; }

            _instance = Object.Instantiate(_cfg.PersistentFxPrefab, parent, false);
            _instance.transform.localPosition = _cfg.PersistentFxOffset;
            _instance.transform.localRotation = Quaternion.identity;

            // NEW: запам'ятали базовий scale як у префаба/інстанса
            _baseLocalScale = _instance.transform.localScale;
            _hasBaseScale = true;

            Debug.Log($"[AuraFx] Spawned instance='{_instance.name}' parent='{parent.name}' localPos={_instance.transform.localPosition} baseLocalScale={_baseLocalScale}");

            // Один прохід: поправили scalingMode і запустили
            foreach (var ps in _instance.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;

                // КЛЮЧ: щоб transform scale впливав на весь ефект
                if (main.scalingMode == ParticleSystemScalingMode.Shape)
                    main.scalingMode = ParticleSystemScalingMode.Hierarchy; // або Local

                Debug.Log($"[AuraFx] PS='{ps.name}' scalingMode={main.scalingMode} startSize={main.startSizeMultiplier}");
                ps.Play(true);
            }
        }

        private void UpdateScale()
        {
            if (_instance == null) return;
            if (_cfg == null) return;
            if (!_hasStats) return;

            float baseRange = Mathf.Max(0.001f, _cfg.PersistentFxBaseRange);
            float k = Mathf.Max(0.01f, _stats.Range) / baseRange;

            // якщо з якихось причин не зловили в Ensure
            if (!_hasBaseScale)
            {
                _baseLocalScale = _instance.transform.localScale;
                _hasBaseScale = true;
            }

            // масштабуємо від базового префабного scale
            _instance.transform.localScale = new Vector3(
                _baseLocalScale.x * k,
                _baseLocalScale.y,
                _baseLocalScale.z * k
            );
        }

        private void Clear()
        {
            if (_instance == null) return;

            Debug.Log($"[AuraFx] Clear instance='{_instance.name}'");

            Object.Destroy(_instance);
            _instance = null;

            _hasStats = false;
            _hasBaseScale = false;
            _baseLocalScale = Vector3.one;
        }
    }
}
