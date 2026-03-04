using UnityEngine;

public class WeaponPresentation : MonoBehaviour, IWeaponPresentation
{
    [SerializeField] private Transform _muzzle;
    public Transform Muzzle => _muzzle;
}