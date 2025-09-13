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
        public void Initialize(Transform target, int damage, bool hasSlowEffect = false, int pierceCount = 1)
        {
            this.target = target;
            this.damage = damage;
            this.hasSlowEffect = hasSlowEffect;
            this.pierceCount = pierceCount;
            useDirection = false;
        }
        public void InitializeWithDirection(int damage, bool hasSlowEffect = false, int pierceCount = 1)
        {
            this.damage = damage;
            this.hasSlowEffect = hasSlowEffect;
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
            transform.position += transform.up * speed * Time.deltaTime;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            /// 工程中设置只和敌人交互
            DamageEnemy(other.GetComponent<HealthComponent>());

            currentPierces++;

            // 如果不是穿透攻击或者已达到穿透上限，销毁子弹
            if (pierceCount <= 1 || currentPierces >= pierceCount)
            {
                DestroyProjectile();
            }
        }
        private void DamageEnemy(HealthComponent healthComponent)
        {
            //FloatingTextManager.Instance.ShowDamage(damage, enemy.position, false);
            healthComponent.TakeDamage(damage); 
            Debug.Log($"子弹对 {healthComponent.name} 造成 {damage} 点伤害");
            if (hasSlowEffect)
            {
                // TODO: 给敌人添加减速效果
                Debug.Log($"敌人 {healthComponent.name} 被减速");
            }
        }
        private void DestroyProjectile()
        {
            // 播放撞击特效
            if (impactEffect != null)
            {
                Instantiate(impactEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}