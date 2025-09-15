using System.Collections.Generic;
using Core;
using Data;
using Enums;
using Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Managers
{
    /// <summary>
    /// 技能管理器 - 管理技能释放、冷却、输入等
    /// </summary>
    public class SkillManager : MonoSingleton<SkillManager>
    {
        [SerializeField] private List<SkillData> allSkills = new List<SkillData>();
        
        [SerializeField] private GameObject lightningEffectPrefab;
        [SerializeField] private GameObject darkImpactEffectPrefab;
        [SerializeField] private GameObject voidVortexEffectPrefab;
        
        [SerializeField] private Texture2D skillCursor;
        
        // 技能冷却字典
        private Dictionary<SkillType, float> skillCooldowns = new Dictionary<SkillType, float>();
        
        // 当前选中的技能
        private SkillData currentSelectedSkill = null;
        private bool isInSkillCastMode = false;
        
        // 地图边界（用于限制技能释放范围）
        private Camera mainCamera;
        private Vector2 mapMinBounds;
        private Vector2 mapMaxBounds;

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

        /// <summary>
        /// 快捷键
        /// </summary>
        private void HandleSkillInput()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TrySelectSkill(SkillType.Lightning);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                TrySelectSkill(SkillType.DarkImpact);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                TrySelectSkill(SkillType.VoidVortex);
            }
            // 右键取消
            else if (Input.GetMouseButtonDown(1) && isInSkillCastMode)
            {
                CancelSkillCast();
            }
        }

        private void TrySelectSkill(SkillType skillType)
        {
            var skillData = GetSkillData(skillType);
            if (skillData == null)
                return;

            if (!ResearchManager.Instance.IsSkillUnlocked(skillType))
            {
                Debug.Log($"技能 {skillData.skillName} 尚未解锁");
                return;
            }

            if (IsSkillOnCooldown(skillType))
            {
                Debug.Log($"技能 {skillData.skillName} 正在冷却中");
                return;
            }

            int cost = skillData.knowledgeCost;
            if (!ResourceManager.Instance.HasEnoughKnowledge(cost))
            {
                Debug.Log($"学识不足，需要 {cost} 学识");
                return;
            }

            currentSelectedSkill = skillData;
            isInSkillCastMode = true;
            
            Cursor.SetCursor(skillCursor, new Vector2(13, 13), CursorMode.Auto);
            
            Debug.Log($"选择技能: {skillData.skillName}");
        }

        /// <summary>
        /// 处理技能施放
        /// </summary>
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

            // 播放音效
            if (skillData.castSound != null)
            {
                // AudioManager.Instance.PlaySFX(skillData.castSound);
            }
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

        public SkillData GetCurrentSelectedSkill()
        {
            return currentSelectedSkill;
        }

        public bool IsInSkillCastMode => isInSkillCastMode;
    }
}