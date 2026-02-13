namespace CodeBase.Hero
{
    public readonly struct DamageRoll
    {
        public readonly float Damage;
        public readonly bool IsCrit;

        public DamageRoll(float damage, bool isCrit)
        {
            Damage = damage;
            IsCrit = isCrit;
        }
    }
}