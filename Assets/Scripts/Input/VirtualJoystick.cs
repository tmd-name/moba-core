using UnityEngine;
using UnityEngine.EventSystems;

namespace MobaCore.Input
{
    /// <summary>
    /// UGUI 虚拟摇杆:按下出现、拖动追踪,输出归一化方向向量。
    /// 挂载在摇杆背景 RectTransform 上,手柄用 handle 引用。
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private RectTransform handle;
        [SerializeField] private float radius = 120f;
        [SerializeField] private float deadZone = 0.15f;
        [SerializeField] private bool dynamicMode = true;

        /// <summary>归一化方向(带模长),死区内为 Vector2.zero。</summary>
        public Vector2 Direction { get; private set; }
        public bool IsPressed { get; private set; }

        private RectTransform _area;
        private Vector2 _origin;

        private void Awake()
        {
            _area = (RectTransform)transform;
            _origin = _area.position;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (dynamicMode)
            {
                _area.position = eventData.position;
                _origin = eventData.position;
            }
            IsPressed = true;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 delta = Vector2.ClampMagnitude(eventData.position - _origin, radius);
            if (handle != null) handle.position = _origin + delta;

            Vector2 dir = delta / radius;
            Direction = dir.magnitude < deadZone ? Vector2.zero : dir;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
            Direction = Vector2.zero;
            if (handle != null) handle.position = _origin;
        }
    }
}