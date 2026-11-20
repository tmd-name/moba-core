namespace MobaCore.Combat
{
    /// <summary>可受伤对象:技能命中结果通过该接口结算,SkillSystem 自身不直接扣血。</summary>
    public interface IDamageable
    {
        void ApplyDamage(float amount, UnityEngine.Object source);
    }
}