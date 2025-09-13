using Core;
using Enums;
using Managers;
using UnityEngine;

namespace Components.Buildings
{
    /// <summary>
    /// 资源生产建筑组件
    /// </summary>
    public class ProductionComponent : MonoBehaviour
    {
        private BuildingComponent buildingComponent;
        private float timer = 0f;

        [Header("生产特效")]
        [SerializeField] private GameObject productionEffect;
        
        private void Awake()
        {
            buildingComponent = GetComponent<BuildingComponent>();
        }
        
        private void Update()
        {
            if (TimeManager.Instance.IsDay && buildingComponent.IsBuilt)
            {
                timer += Time.deltaTime;
                if (timer >= buildingComponent.Data.ProductionInterval)
                {
                    ProduceResource();
                    timer = 0f;
                }
            }
        }
        
        private void ProduceResource()
        {
            var data = buildingComponent.Data;

            int finalAmount = ResourceManager.Instance.CalculateFinalProduction(data.LevelDatas[buildingComponent.LevelIndex].BaseProduction);

            // 添加资源
            switch (data.ResourceType)
            {
                case ResourceType.Gold:
                    ResourceManager.Instance.AddGold(finalAmount);
                    break;
                case ResourceType.Knowledge:
                    ResourceManager.Instance.AddKnowledge(finalAmount);
                    break;
            }

            // 显示生产特效
            if (productionEffect != null)
            {
                var effect = Instantiate(productionEffect, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // 显示飘字提示
            ShowProductionFloatingText($"+{finalAmount}");
            
            Debug.Log($"{data.BuildingName} 生产了 {finalAmount} {data.ResourceType}");
        }
        
        /// <summary>
        /// 显示生产飘字
        /// </summary>
        private void ShowProductionFloatingText(string text)
        {
            if (FloatingTextManager.Instance == null) return;
            
            // 转换资源类型为本地化字符串
            //string resourceName = GetLocalizedResourceName(resourceType);
            
            // 在建筑上方显示飘字
            Vector3 floatingTextPosition = transform.position + Vector3.up * 0.5f;
            FloatingTextManager.Instance.ShowResourceProduction(text, floatingTextPosition);
        }
        
        /// <summary>
        /// 获取本地化的资源名称
        /// </summary>
        private string GetLocalizedResourceName(ResourceType resourceType)
        {
            // 这里可以根据语言设置返回不同的文本
            // 暂时使用中文
            switch (resourceType)
            {
                case ResourceType.Gold:
                    return "金币";
                case ResourceType.Knowledge:
                    return "学识";
                case ResourceType.Food:
                    return "食物";
                default:
                    return resourceType.ToString();
            }
        }
    }
}