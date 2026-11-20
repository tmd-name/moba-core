using System;
using System.Collections;
using UnityEngine;
using MobaCore.Combat;

namespace MobaCore.Skills
{
    public enum CastFailReason
    {
        None = 0,
        NoSkill,
        OnCooldown,
        NotEnoughMana,
        Silenced,
        Busy
    }

    public struct CastResult
    {
        public bool Success;
        public CastFailReason Reason;
    }

    /// <summary>
    /// 施法管线:合法性校验 → 前摇 → 范围命中判定 → 冷却/蓝耗结算。
    /// 命中结果通过事件抛给伤害与表现层。
    /// </summary>
    public class SkillSystem : MonoBehaviour
    {
        [Serializable]
        public class SkillSlot
        {
            public SkillDefinition definition;
            [NonSerialized] public float cooldownRemaining;
            [NonSerialized] public bool isCasting;
        }

        [SerializeField] private SkillSlot[] slots = new SkillSlot[3];
        [SerializeField] private Transform castOrigin;

        [Header("Resources")]
        [SerializeField] private float maxMana = 200f;
        [SerializeField] private float currentMana = 200f;

        public int SlotCount => slots != null ? slots.Length : 0;
        public bool Silenced { get; set; }

        public event Action<SkillSlot> OnCastStarted;
        public event Action<SkillSlot, Collider[]> OnCastSucceeded;
        public event Action<SkillSlot, CastFailReason> OnCastFailed;

        private readonly Collider[] _hits = new Collider[32];

        private void Update()
        {
            if (slots == null) return;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].cooldownRemaining > 0f)
                    slots[i].cooldownRemaining -= Time.deltaTime;
            }
        }

        /// <summary>尝试释放 skills[slotIndex],aimPoint 为目标点或方向终点。</summary>
        public CastResult TryCast(int slotIndex, Vector3 aimPoint)
        {
            SkillSlot slot = slotIndex >= 0 && slotIndex < SlotCount ? slots[slotIndex] : null;
            if (slot == null || slot.definition == null) return Reject(slot, CastFailReason.NoSkill);
            if (slot.cooldownRemaining > 0f) return Reject(slot, CastFailReason.OnCooldown);
            if (slot.isCasting) return Reject(slot, CastFailReason.Busy);
            if (Silenced) return Reject(slot, CastFailReason.Silenced);
            if (currentMana < slot.definition.manaCost) return Reject(slot, CastFailReason.NotEnoughMana);

            StartCoroutine(CastRoutine(slot, aimPoint));
            return new CastResult { Success = true, Reason = CastFailReason.None };
        }

        /// <summary>冷却进度 0~1,供 UI 读条。</summary>
        public float GetCooldownRatio(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount) return 0f;
            SkillSlot slot = slots[slotIndex];
            if (slot?.definition == null || slot.definition.cooldown <= 0f) return 0f;
            return Mathf.Clamp01(slot.cooldownRemaining / slot.definition.cooldown);
        }

        private IEnumerator CastRoutine(SkillSlot slot, Vector3 aimPoint)
        {
            SkillDefinition def = slot.definition;
            slot.isCasting = true;
            OnCastStarted?.Invoke(slot);

            if (def.castTime > 0f) yield return new WaitForSeconds(def.castTime);

            // 前摇结束:扣蓝、进冷却、结算命中
            currentMana -= def.manaCost;
            slot.cooldownRemaining = def.cooldown;
            slot.isCasting = false;

            Collider[] targets = ResolveTargets(def, aimPoint);
            OnCastSucceeded?.Invoke(slot, targets);
        }

        /// <summary>按施法类型做范围命中判定;返回有效目标列表。</summary>
        private Collider[] ResolveTargets(SkillDefinition def, Vector3 aimPoint)
        {
            Vector3 origin = castOrigin != null ? castOrigin.position : transform.position;
            Vector3 center;
            LayerMask mask = def.targetMask;

            if (def.castType == CastType.Self)
            {
                center = origin;
            }
            else
            {
                Vector3 toAim = aimPoint - origin;
                toAim.y = 0f;
                if (toAim.magnitude > def.range) toAim = toAim.normalized * def.range;
                center = origin + toAim;
            }

            int count = Physics.OverlapSphereNonAlloc(center, def.radius, _hits, mask, QueryTriggerInteraction.Ignore);

            if (count == 0) return Array.Empty<Collider>();

            var valid = new Collider[count];
            int n = 0;
            for (int i = 0; i < count; i++)
            {
                Collider hit = _hits[i];
                if (hit == null || hit.transform.IsChildOf(transform) && def.castType != CastType.Self) continue;
                valid[n++] = hit;

                if (hit.TryGetComponent<IDamageable>(out var damageable))
                    damageable.ApplyDamage(def.damage, this);
            }

            if (n == valid.Length) return valid;
            var trimmed = new Collider[n];
            Array.Copy(valid, trimmed, n);
            return trimmed;
        }

        private CastResult Reject(SkillSlot slot, CastFailReason reason)
        {
            OnCastFailed?.Invoke(slot, reason);
            return new CastResult { Success = false, Reason = reason };
        }
    }
}