using UnityEngine;

namespace MobaCore.Skills
{
    /// <summary>施法类型:自身 / 单位指向 / 地面指向 / 方向技能。</summary>
    public enum CastType
    {
        Self = 0,
        Unit = 1,
        Point = 2,
        Direction = 3
    }

    /// <summary>技能配置:施法类型、范围、冷却、蓝耗、前摇与伤害参数。</summary>
    [CreateAssetMenu(menuName = "MobaCore/Skill Definition", fileName = "Skill_")]
    public class SkillDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string skillId = "skill_001";
        public string displayName = "测试技能";

        [Header("Casting")]
        public CastType castType = CastType.Direction;
        [Tooltip("前摇时长(秒)")]
        public float castTime = 0.2f;
        [Tooltip("指示器范围")]
        public float range = 6f;
        [Tooltip("生效半径(范围技能)")]
        public float radius = 2f;

        [Header("Cost & Cooldown")]
        public float manaCost = 30f;
        public float cooldown = 5f;

        [Header("Effect")]
        public float damage = 100f;
        public LayerMask targetMask;
    }
}