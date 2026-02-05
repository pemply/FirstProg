namespace CodeBase.Enemy
{
    public interface IEnemyAttackApplier
    {
        void ApplyStats(float damage, float cooldown, float cleavage, float effectiveDistance);
    }
}