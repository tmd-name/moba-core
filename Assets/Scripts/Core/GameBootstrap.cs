using UnityEngine;
using MobaCore.Movement;
using MobaCore.Skills;

namespace MobaCore.Core
{
    /// <summary>场景入口:装配玩家移动与技能系统,施法事件薄给 UI/表现层。</summary>
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private PlayerMotor playerMotor;
        [SerializeField] private SkillSystem playerSkills;
        [SerializeField] private Transform aimTarget;

        private void OnEnable()
        {
            if (playerSkills == null) return;
            playerSkills.OnCastStarted += slot => Debug.Log($"[Skill] 开始施法: {slot.definition.displayName}");
            playerSkills.OnCastSucceeded += (slot, hits) => Debug.Log($"[Skill] {slot.definition.displayName} 命中 {hits.Length} 个目标");
            playerSkills.OnCastFailed += (slot, reason) => Debug.Log($"[Skill] 施法被拒: {reason}");
        }

        private void OnDisable()
        {
            if (playerSkills == null) return;
            playerSkills.OnCastStarted = null;
            playerSkills.OnCastSucceeded = null;
            playerSkills.OnCastFailed = null;
        }

        // 原型期测试入口:Q 键朝准星方向释放 1 号位技能
        private void Update()
        {
            if (playerSkills == null) return;
            if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
            {
                Vector3 aim = aimTarget != null
                    ? aimTarget.position
                    : (playerMotor != null ? playerMotor.transform.position + playerMotor.transform.forward * 5f : transform.position + transform.forward * 5f);
                playerSkills.TryCast(0, aim);
            }
        }
    }
}