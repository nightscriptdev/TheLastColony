using Components.Enemies;

namespace Game
{
    public class SlowEffect : StatusEffect
    {
        public float slowMultiplier;

        public SlowEffect(float duration, float multiplier) : base(duration)
        {
            this.slowMultiplier = multiplier;
        }

        public override void Apply(EnemyComponent enemy)
        {
            enemy.RecalculateStats();
        }

        public override void Remove(EnemyComponent enemy)
        {
            enemy.RecalculateStats();
        }
    }
}