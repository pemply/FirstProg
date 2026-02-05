using CodeBase.StaticData;

namespace CodeBase.Weapon
{
    public interface IWeaponPresentation
    {
        void PlayAttack(in WeaponStats stats);
        void ApplyStats(in WeaponStats stats); // щоб зона/VFX могли підлаштовуватись
    }
}