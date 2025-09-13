using System.Collections;
using UnityEngine;

namespace Test
{
    public class MagicTower : MonoBehaviour
    {
        public float aniPercent = 1f;
        
        [Header("基本设置")]
        public GameObject bulletPrefab;          // 子弹预制体
        public Transform firePoint;              // 发射点
        public float bulletSpeed = 10f;          // 子弹速度
    
        [Header("动画")]
        public Animator animator;                // 动画控制器
    
        private Camera mainCamera;
    
        void Start()
        {
            mainCamera = Camera.main;
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
                StartCoroutine(ChargeAndFire());
        }
        
        IEnumerator ChargeAndFire()
        {
            if (animator != null)
            {
                var renderer = animator.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.enabled = true;  // 播放前显示

                animator.Play(0);
                yield return null;

                yield return new WaitUntil(() =>
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    return stateInfo.normalizedTime >= aniPercent && stateInfo.IsName("Charge");
                });
                
                FireBullet();
                
                yield return new WaitUntil(() =>
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    return stateInfo.normalizedTime >= 1 && stateInfo.IsName("Charge");
                });

                if (renderer != null) renderer.enabled = false; // 播放完隐藏
            }
            else
            {
                FireBullet();

            }

           
        }
    
        void FireBullet()
        {
            if (bulletPrefab == null || firePoint == null) return;
        
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            Vector2 direction = (worldPos - firePoint.position).normalized;
            
            // 计算子弹旋转角度，让子弹的y轴负方向与飞行方向一致
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            Quaternion bulletRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        
            if (rb != null)
            {
                rb.velocity = direction * bulletSpeed;
            }
        
            // 5秒后销毁子弹
            Destroy(bullet, 5f);
        }
    }
}