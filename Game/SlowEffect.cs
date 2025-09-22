using Components.Enemies;

namespace Game
{
    public class SlowEffect : StatusEffect
    {
        public float slowMultiplier; // 减速乘数，例如 0.5 代表减速50%

        // 构造函数，允许传入持续时间和减速程度
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