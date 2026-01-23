using CodeBase.StaticData;

namespace CodeBase.Logic.Upgrade
{
    public struct UpgradeRoll
    {
        public UpgradeConfig Config;
        public UpgradeRarity Rarity;

        public float FloatValue;
        public int IntValue;

        public WeaponId WeaponPreviewId;
    }
}