using UnityEngine;

namespace MobaCore.Network
{
    /// <summary>状态快照:位置/朝向/速度,后续状态同步与回滚的基础单元。</summary>
    public struct StateSnapshot
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
    }

    /// <summary>可快照对象:本地测与未来服务端状态同步的对接接口。</summary>
    public interface IStateSnapshotProvider
    {
        StateSnapshot CaptureSnapshot();
        void ApplySnapshot(StateSnapshot snapshot);
    }
}