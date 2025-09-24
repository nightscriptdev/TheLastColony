using Data;
using UnityEngine;

namespace Test
{
    public class Test2 : MonoBehaviour
    {
        public SkillData skillData;
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.LogError( skillData.knowledgeCost);
            }
        }
    }
}