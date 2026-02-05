using UnityEngine;

namespace CodeBase.Weapon
{
    public class WeaponSlots : MonoBehaviour
    {
        public Transform Primary;
        public Transform[] Secondary; // [0] = slot1, [1] = slot2, ...
    }
}