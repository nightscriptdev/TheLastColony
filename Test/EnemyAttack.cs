using System.Collections;
using UnityEngine;

namespace Test
{
    public class EnemyAttack : MonoBehaviour
    {
        [Header("攻击设置")]
        public float attackSpeed = 10f;         // 攻击移动速度
        public float returnSpeed = 5f;          // 返回速度
        
        private Camera mainCamera;
        private Vector3 originalPosition;       // 原始位置
        private bool isAttacking = false;       // 是否正在攻击
        
        void Start()
        {
            mainCamera = Camera.main;
            originalPosition = transform.position;
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !isAttacking)
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
                worldPos.z = transform.position.z;
                
                StartCoroutine(AttackMove(worldPos));
            }
        }
        
        IEnumerator AttackMove(Vector3 targetPosition)
        {
            isAttacking = true;
            
            // 移动到目标位置
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, attackSpeed * Time.deltaTime);
                yield return null;
            }
            
            // 返回原始位置
            while (Vector3.Distance(transform.position, originalPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, originalPosition, returnSpeed * Time.deltaTime);
                yield return null;
            }
            
            transform.position = originalPosition;
            isAttacking = false;
        }
    }
}