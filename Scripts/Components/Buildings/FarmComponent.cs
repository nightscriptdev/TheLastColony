using Core;
using Managers;
using UnityEngine;

namespace Components.Buildings
{
    public class FarmComponent : MonoBehaviour
    {
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private BuildingComponent buildingComponent;
        
        private int _spriteIndex;
        private float timer = 0f;
        private float spriteTimer = 0f;
        
        [Header("生产特效")]
        [SerializeField] private GameObject productionEffect;
        
        private void Update()
        {
            if (TimeManager.Instance.IsDay && buildingComponent.IsBuilt)
            {
                timer += Time.deltaTime;
                spriteTimer += Time.deltaTime;
                
                if (spriteTimer >= 10f)
                {
                    ChangeSprite();
                    spriteTimer = 0f;
                }
                
                // 生产资源
                if (timer >= buildingComponent.Data.ProductionInterval)
                {
                    ProduceResource();
                    timer = 0f;
                }
            }
        }
        
        void ChangeSprite()
        {
            if (_sprites == null || _sprites.Length == 0) return;
            
            if (++_spriteIndex >= _sprites.Length) 
                _spriteIndex = 0;
            _spriteRenderer.sprite = _sprites[_spriteIndex];
        }
        
        private void ProduceResource()
        {
            var data = buildingComponent.Data;
            int finalAmount = ResourceManager.Instance.CalculateFinalProduction(data.LevelDatas[buildingComponent.LevelIndex].BaseProduction);
            ResourceManager.Instance.AddFood(finalAmount);
            
            // 显示生产特效
            if (productionEffect != null)
            {
                var effect = Instantiate(productionEffect, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // 显示飘字提示
            ShowProductionFloatingText(finalAmount);
            
            Debug.Log($"{data.BuildingName} 生产了 {finalAmount} {data.ResourceType}");
        }
        
        /// <summary>
        /// 显示生产飘字
        /// </summary>
        private void ShowProductionFloatingText(int amount)
        {
            if (FloatingTextManager.Instance == null) return;
            
            // 在建筑上方显示飘字
            Vector3 floatingTextPosition = transform.position + Vector3.up * 0.5f;
            FloatingTextManager.Instance.ShowResourceProduction($"+{amount}", floatingTextPosition);
        }
    }
}