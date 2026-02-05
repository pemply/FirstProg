using CodeBase.Infrastructure.Factory;
using CodeBase.StaticData;

namespace CodeBase.Weapon
{
    public interface IWeaponIdReceiver
    {
        void SetWeaponId(WeaponId id);
    }

    public interface IProjectileFactoryReceiver
    {
        void Construct(ProjectileFactory factory);
    }
}