using Components.Enemies;

namespace Game
{
    public abstract class StatusEffect
    {
        public float duration;
        public float remainingTime;
    
        public StatusEffect(float duration)
        {
            this.duration = duration;
            this.remainingTime = duration;
        }

        public abstract void Apply(EnemyComponent enemy);
        public abstract void Remove(EnemyComponent enemy);
        public virtual void Update(float deltaTime)
        {
            remainingTime -= deltaTime;
        }
    }
}