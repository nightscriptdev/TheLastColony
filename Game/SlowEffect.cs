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
            // 只通知敌人重新计算属性，不直接修改
            enemy.RecalculateStats();
        }

        public override void Remove(EnemyComponent enemy)
        {
            // 移除时也一样
            enemy.RecalculateStats();
        }
    }
}