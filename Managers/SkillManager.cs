using System.Collections.Generic;
using Core;
using Data;
using Enums;
using Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Managers
{
    public class SkillManager : MonoSingleton<SkillManager>
    {
        [SerializeField] private List<SkillData> allSkills = new List<SkillData>();
        
        [SerializeField] private GameObject lightningEffectPrefab;
        [SerializeField] private GameObject darkImpactEffectPrefab;
        [SerializeField] private GameObject voidVortexEffectPrefab;
        
        [SerializeField] private Texture2D skillCursor;
        
        private Dictionary<SkillType, float> skillCooldowns = new Dictionary<SkillType, float>();
        
        private SkillData currentSelectedSkill = null;
        private bool isInSkillCastMode = false;
        
        private Camera mainCamera;

        protected override void Awake()
        {
            base.Awake();
            mainCamera = Camera.main;
            
            InitializeSkillCooldowns();
        }

        void Update()
        {
            if (!GameManager.Instance.IsPlaying)
                return;

            HandleSkillInput();
            UpdateCooldowns();
            HandleSkillCasting();
        }

        private void InitializeSkillCooldowns()
        {
            skillCooldowns[SkillType.Lightning] = 0f;
            skillCooldowns[SkillType.DarkImpact] = 0f;
            skillCooldowns[SkillType.VoidVortex] = 0f;
        }

        private void HandleSkillInput()
        {
            foreach (var skillData in allSkills)
            {
                if (Input.GetKeyDown(skillData.hotkey))
                {
                    TrySelectSkill(skillData.skillType);
                    break; // 找到匹配的就退出
                }
            }
            // 右键取消
            if (Input.GetMouseButtonDown(1) && isInSkillCastMode)
            {
                CancelSkillCast();
            }
        }

        public void TrySelectSkill(SkillType skillType)
        {
            var skillData = GetSkillData(skillType);
            if (skillData == null)
                return;

            if (!ResearchManager.Instance.IsSkillUnlocked(skillType))
            {
                return;
            }

            if (IsSkillOnCooldown(skillType))
            {
                return;
            }

            int cost = skillData.knowledgeCost;
            if (!ResourceManager.Instance.HasEnoughKnowledge(cost))
            {
                return;
            }

            currentSelectedSkill = skillData;
            isInSkillCastMode = true;
            
            //Cursor.SetCursor(skillCursor, new Vector2(12.5f, 0), CursorMode.Auto);
            Cursor.SetCursor(skillCursor, new Vector2(13f, 13f), CursorMode.Auto);
            
        }

        private void HandleSkillCasting()
        {
            if (!isInSkillCastMode || currentSelectedSkill == null)
                return;

            // 左键释放技能
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    CastSkill(currentSelectedSkill, mouseWorldPos);
                    CancelSkillCast();
                }
            }
        }

        private void CastSkill(SkillData skillData, Vector3 position)
        {
            if (!ResourceManager.Instance.SpendKnowledge(skillData.knowledgeCost))
                return;

            skillCooldowns[skillData.skillType] = skillData.cooldownTime;

            CreateSkillEffect(skillData, position);
        }

        private void CreateSkillEffect(SkillData skillData, Vector3 position)
        {
            GameObject effectPrefab = GetEffectPrefab(skillData.skillType);
            if (effectPrefab == null)
                return;

            GameObject effectObj = Instantiate(effectPrefab, position, Quaternion.identity);
            SkillEffect effect = effectObj.GetComponent<SkillEffect>();
            
            if (effect != null)
            {
                effect.Initialize(skillData, position);
            }
        }

        private GameObject GetEffectPrefab(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.Lightning => lightningEffectPrefab,
                SkillType.DarkImpact => darkImpactEffectPrefab,
                SkillType.VoidVortex => voidVortexEffectPrefab,
                _ => null
            };
        }

        private void CancelSkillCast()
        {
            isInSkillCastMode = false;
            currentSelectedSkill = null;
            
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void UpdateCooldowns()
        {
            var keys = new List<SkillType>(skillCooldowns.Keys);
            foreach (var skillType in keys)
            {
                if (skillCooldowns[skillType] > 0)
                {
                    skillCooldowns[skillType] -= Time.deltaTime;
                    if (skillCooldowns[skillType] <= 0)
                    {
                        skillCooldowns[skillType] = 0;
                    }
                }
            }
        }

        public bool IsSkillOnCooldown(SkillType skillType)
        {
            return skillCooldowns.ContainsKey(skillType) && skillCooldowns[skillType] > 0;
        }

        public float GetSkillCooldown(SkillType skillType)
        {
            return skillCooldowns.ContainsKey(skillType) ? skillCooldowns[skillType] : 0f;
        }

        public SkillData GetSkillData(SkillType skillType)
        {
            return allSkills.Find(s => s.skillType == skillType);
        }
    }
}