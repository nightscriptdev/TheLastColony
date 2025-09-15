using Data;
using UnityEngine;

namespace Game
{
    public abstract class SkillEffect : MonoBehaviour
    {
        protected SkillData skillData;
        protected Vector3 targetPosition;

        public virtual void Initialize(SkillData data, Vector3 position)
        {
            skillData = data;
            targetPosition = position;
            if (skillData.effectRadius != 0)
            {
                float scale = skillData.effectRadius * 2;
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        public virtual void OnComplete()
        {
            Destroy(gameObject);
        }
    }
}