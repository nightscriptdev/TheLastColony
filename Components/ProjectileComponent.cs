using Components.Enemies;
using Core;
using Game;
using UnityEngine;

namespace Components
{
    public class ProjectileComponent : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float duration = 2f;
        [SerializeField] private GameObject impactEffect;
        private Vector2 direction;
        private int damage;
        private bool hasSlowEffect;
        private int pierceCount = 1;
        private int currentPierces = 0;
        
        public void Initialize(int damage, bool hasSlowEffect = false, int pierceCount = 1)
        {
            this.damage = damage;
            this.hasSlowEffect = hasSlowEffect;
            this.pierceCount = pierceCount;
            currentPierces = 0;
            
            if (duration > 0f)
                Invoke(nameof(Release), duration);
        }
        private void Update()
        {
            MoveInDirection();
        }
        private void MoveInDirection()
        {
            transform.position += transform.right * speed * Time.deltaTime;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            /// 工程中设置只和敌人交互
            other.GetComponent<HealthComponent>().TakeDamage(damage);

            if (hasSlowEffect)
            {
                other.GetComponent<EnemyComponent>().ApplyStatusEffect(new SlowEffect(3.0f, 0.5f));
            }
            
            if (impactEffect != null)
            {
                PoolingManager.Instance.Get(impactEffect).transform.SetPositionAndRotation(other.ClosestPoint(transform.position), Quaternion.identity);
            }
            
            currentPierces++;

            if (pierceCount <= 1 || currentPierces >= pierceCount)
            {
                Release();
            }
        }
        void Release()
        {
            if (!gameObject) return;
            CancelInvoke();
            PoolingManager.Instance.ReleaseProjectile(this);
        }
    }
}