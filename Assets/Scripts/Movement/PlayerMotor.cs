using UnityEngine;
using MobaCore.Input;
using MobaCore.Network;

namespace MobaCore.Movement
{
    /// <summary>
    /// 玩家移动控制:摇杆输入 → 相机坐标系转换 → 平滑加速 → FixedUpdate 位移。
    /// 实现 IStateSnapshotProvider,为后续网络状态同步预留快照接口。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour, IStateSnapshotProvider
    {
        [SerializeField] private VirtualJoystick joystick;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float turnSpeed = 720f;

        private CharacterController _controller;
        private Vector3 _velocity;
        private Vector2 _input;

        /// <summary>被控制/眩晕时置 false。</summary>
        public bool CanMove { get; set; } = true;

        private void Awake() => _controller = GetComponent<CharacterController>();

        private void Update()
        {
            _input = CanMove && joystick != null ? joystick.Direction : Vector2.zero;
        }

        private void FixedUpdate()
        {
            Vector3 wish = CameraRelative(_input);
            _velocity = Vector3.MoveTowards(_velocity, wish * moveSpeed, acceleration * Time.fixedDeltaTime);

            if (_controller.isGrounded) _velocity.y = 0f;
            else _velocity.y += Physics.gravity.y * Time.fixedDeltaTime;

            _controller.Move(_velocity * Time.fixedDeltaTime);

            if (wish.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(wish);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.fixedDeltaTime);
            }
        }

        /// <summary>摇杆方向 → 相机水平坐标系下的世界方向。</summary>
        private Vector3 CameraRelative(Vector2 input)
        {
            if (input.sqrMagnitude < 0.0001f) return Vector3.zero;

            Transform cam = cameraTransform != null
                ? cameraTransform
                : (Camera.main != null ? Camera.main.transform : null);

            Vector3 fwd = cam != null ? cam.forward : Vector3.forward;
            Vector3 right = cam != null ? cam.right : Vector3.right;
            fwd.y = 0f; right.y = 0f;
            fwd.Normalize(); right.Normalize();

            return Vector3.ClampMagnitude(fwd * input.y + right * input.x, 1f);
        }

        public StateSnapshot CaptureSnapshot() => new StateSnapshot
        {
            Position = transform.position,
            Rotation = transform.rotation,
            Velocity = _velocity
        };

        public void ApplySnapshot(StateSnapshot snapshot)
        {
            _controller.enabled = false;
            transform.SetPositionAndRotation(snapshot.Position, snapshot.Rotation);
            _controller.enabled = true;
            _velocity = snapshot.Velocity;
        }
    }
}