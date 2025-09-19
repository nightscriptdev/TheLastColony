using UnityEngine;

namespace Components.Buildings
{
    public class GrowthStageComponent : MonoBehaviour
    {
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ProductionComponent productionComponent;
        
        private int _spriteIndex;
        private float stageDuration;
        private float spriteTimer = 0f;
        
        private void Awake()
        {
            if(productionComponent == null)
                productionComponent = GetComponent<ProductionComponent>();
            stageDuration = productionComponent.buildingComponent.Data.ProductionInterval / _sprites.Length;
        }
        
        private void Update()
        {
            if (productionComponent.IsProducing)
            {
                spriteTimer += Time.deltaTime;
                
                if (spriteTimer >= stageDuration)
                {
                    ChangeSprite();
                    spriteTimer = 0;
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
    }
}