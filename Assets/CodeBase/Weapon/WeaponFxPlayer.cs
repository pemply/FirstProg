using CodeBase.Infrastructure.Services.Pool;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class WeaponFxPlayer : MonoBehaviour, IWeaponConfigReceiver
    {
        private WeaponConfig _cfg;
        private IPoolService _pool;

        public void Construct(IPoolService pool) => _pool = pool;
        public void SetConfig(WeaponConfig cfg) => _cfg = cfg;

        public void PlayAttackFx(Vector3 origin, Vector3 forward, float range, float hitWidth)
        {
            if (_cfg == null || _cfg.AttackFxPrefab == null) return;
            if (_pool == null) { Debug.LogError("[WeaponFxPlayer] Pool not constructed", this); return; }

            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;

            Quaternion rot = Quaternion.LookRotation(forward.normalized, Vector3.up);
            Vector3 pos = origin + rot * _cfg.AttackFxOffset;

            var fx = _pool.Get(_cfg.AttackFxPrefab, pos, rot);

            // scale
            Vector3 baseScale = _cfg.AttackFxPrefab.transform.localScale;

            float baseRange = Mathf.Max(0.01f, _cfg.AttackFxBaseRange);
            float baseWidth = Mathf.Max(0.01f, _cfg.AttackFxBaseWidth);
            float k = Mathf.Max(0.001f, _cfg.AttackFxScaleMult);

            float rangeScale = Mathf.Max(0.1f, range / baseRange);
            float widthScale = Mathf.Max(0.1f, hitWidth / baseWidth);

            fx.transform.localScale = new Vector3(
                baseScale.x * widthScale * k,
                baseScale.y * k,
                baseScale.z * rangeScale * k
            );
        }
    }
}