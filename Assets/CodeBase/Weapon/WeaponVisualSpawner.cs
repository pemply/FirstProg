using System.Collections.Generic;
using CodeBase.Weapon;
using UnityEngine;

namespace CodeBase.Hero
{
    public class WeaponVisualSpawner : MonoBehaviour
    {
        [SerializeField] private WeaponSlots _slots;

        private GameObject _primaryInstance;
        private readonly List<GameObject> _secondaryInstances = new();

        public IWeaponPresentation PrimaryPresentation { get; private set; }
        public IReadOnlyList<IWeaponPresentation> SecondaryPresentations => _secondaryPresentations;
        private readonly List<IWeaponPresentation> _secondaryPresentations = new();

        // ✅ NEW: щоб не пересоздавати кожен ApplyCurrent
        private GameObject _primaryPrefabUsed;

        public IWeaponPresentation SpawnPrimary(GameObject prefab)
        {
            // ✅ NEW: якщо той самий префаб уже в руках — нічого не робимо
            if (_primaryInstance != null && _primaryPrefabUsed == prefab)
                return PrimaryPresentation;

            ClearPrimary();
            if (prefab == null) return null;

            _primaryInstance = Instantiate(prefab, _slots.Primary);
            ResetLocal(_primaryInstance.transform);

            _primaryPrefabUsed = prefab; // ✅ NEW
            PrimaryPresentation = _primaryInstance.GetComponentInChildren<IWeaponPresentation>(true);
            return PrimaryPresentation;
        }

        // ✅ NEW: показати "зброю в руках" за правилом:
        // беремо первий НЕ-null prefab у списку (0..n-1)
        public IWeaponPresentation SpawnPrimaryFromList(IReadOnlyList<GameObject> prefabsInOrder)
        {
            if (prefabsInOrder == null || prefabsInOrder.Count == 0)
                return SpawnPrimary(null);

            for (int i = 0; i < prefabsInOrder.Count; i++)
            {
                var prefab = prefabsInOrder[i];
                if (prefab != null)
                    return SpawnPrimary(prefab);
            }

            return SpawnPrimary(null);
        }

        public IWeaponPresentation SpawnSecondary(int slotIndex, GameObject prefab)
        {
            if (prefab == null) return null;
            if (_slots == null || _slots.Secondary == null) return null;
            if (slotIndex < 0 || slotIndex >= _slots.Secondary.Length) return null;
            if (_slots.Secondary[slotIndex] == null) return null;

            EnsureSecondaryCapacity(slotIndex + 1);

            if (_secondaryInstances[slotIndex] != null)
                Destroy(_secondaryInstances[slotIndex]);

            var go = Instantiate(prefab, _slots.Secondary[slotIndex]);
            ResetLocal(go.transform);

            _secondaryInstances[slotIndex] = go;

            var pres = go.GetComponentInChildren<IWeaponPresentation>(true);
            _secondaryPresentations[slotIndex] = pres;
            return pres;
        }

        public void ClearPrimary()
        {
            if (_primaryInstance != null) Destroy(_primaryInstance);
            _primaryInstance = null;
            PrimaryPresentation = null;

            _primaryPrefabUsed = null; // ✅ NEW
        }

        public void ClearAllSecondary()
        {
            for (int i = 0; i < _secondaryInstances.Count; i++)
                if (_secondaryInstances[i] != null)
                    Destroy(_secondaryInstances[i]);

            _secondaryInstances.Clear();
            _secondaryPresentations.Clear();
        }

        private void EnsureSecondaryCapacity(int size)
        {
            while (_secondaryInstances.Count < size) _secondaryInstances.Add(null);
            while (_secondaryPresentations.Count < size) _secondaryPresentations.Add(null);
        }

        private static void ResetLocal(Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
    }
}
