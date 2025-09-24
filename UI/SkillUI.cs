using UnityEngine;

namespace UI
{
    public class SkillUI : MonoBehaviour
    {
        [SerializeField] private Transform skillButtonContainer;

        public void Hide()
        {
            skillButtonContainer.gameObject.SetActive(false);
        }
    }
}