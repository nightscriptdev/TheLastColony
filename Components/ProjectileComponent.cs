using Components.Enemies;
using Game;
using UnityEngine;

namespace Components
{
    /// <summary>
    /// 投射物组件
    /// </summary>
    public class ProjectileComponent : MonoBehaviour
    {
        [Header("移动设置")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 5f;
        [Header("视觉效果")]
        [SerializeField] private GameObject impactEffect;
        private Transform target;
        private Vector2 direction;
        private int damage;
        private bool hasSlowEffect;
        private int pierceCount = 1;
        private int currentPierces = 0;
        private bool useDirection = false;
        private void Start()
        {
            // 自动销毁
            Destroy(gameObject, lifetime);
        }
        /*public void Initialize(Transform target, int damage, bool hasSlowEffect = false, int pierceCount = 1)
        {
            this.target = target;
            this.damage = damage;
            this.hasSlowEffect = hasSlowEffect;
            this.pierceCount = pierceCount;
            useDirection = false;
        }*/
        public void Initialize(int damage, bool hasSlowEffect = false, int pierceCount = 1)
        {
            this.damage = damage;
            this.hasSlowEffect = hasSlowEffect;
            this.pierceCount = pierceCount;
            useDirection = true;
        }
        private void Update()
        {
            if (useDirection)
            {
                MoveInDirection();
            }
            else
            {
                MoveToTarget();
            }
        }
        private void MoveToTarget()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }
            Vector2 direction = (target.position - transform.position).normalized;
            transform.position += (Vector3)direction * speed * Time.deltaTime;
            // 旋转朝向目标
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle- 90f, Vector3.forward);
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
                var obj = Instantiate(impactEffect, other.ClosestPoint(transform.position), Quaternion.identity);
                Destroy(obj, 0.208f); 
            }
            
            currentPierces++;

            // 如果不是穿透攻击或者已达到穿透上限，销毁子弹
            if (pierceCount <= 1 || currentPierces >= pierceCount)
            {
                DestroyProjectile();
            }
        }
        private void DestroyProjectile()
        {
            
            Destroy(gameObject);
        }
    }
}