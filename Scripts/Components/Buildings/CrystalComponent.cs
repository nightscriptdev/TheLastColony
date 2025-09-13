using UnityEngine;

namespace Components.Buildings
{
    /// <summary>
    /// 拒敌水晶组件
    /// </summary>
    public class CrystalComponent : MonoBehaviour
    {
        private BuildingComponent buildingComponent;
        private void Awake()
        {
            buildingComponent = GetComponent<BuildingComponent>();
        }
        private void OnEnable()
        {
            var hpComponent = GetComponent<Components.HealthComponent>();
            if (hpComponent != null)
            {
                hpComponent.OnTakeDamage += OnCrystalDamaged;
            }
        }
        private void OnDisable()
        {
            var hpComponent = GetComponent<Components.HealthComponent>();
            if (hpComponent != null)
            {
                hpComponent.OnTakeDamage -= OnCrystalDamaged;
            }
        }
        private void OnCrystalDamaged(int damage)
        {
            // 对攻击者反射伤害
            // TODO: 需要知道攻击者是谁，这里需要修改HP系统来传递攻击者信息
            /*int reflectDamage = Mathf.RoundToInt(damage * buildingComponent.Data.ReflectPercent);
            Debug.Log($"拒敌水晶反射了 {reflectDamage} 点伤害");*/
        }
    }
}
