using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private RectTransform background; // 摇杆背景
    private RectTransform handle; // 摇杆手柄

    private Vector2 inputDirection; // 输入方向

    private void Awake()
    {
        background = GetComponent<RectTransform>();
        handle = transform.GetChild(0).GetComponent<RectTransform>(); // 摇杆手柄是背景的子元素
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 touchPosition = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out touchPosition))
        {
            // 获取触摸位置相对于摇杆背景的百分比
            touchPosition.x = (touchPosition.x / background.sizeDelta.x) * 2;
            touchPosition.y = (touchPosition.y / background.sizeDelta.y) * 2;
            touchPosition = (touchPosition.magnitude > 1f) ? touchPosition.normalized : touchPosition;

            // 更新摇杆手柄的位置
            handle.anchoredPosition = new Vector2(touchPosition.x * (background.sizeDelta.x / 2), touchPosition.y * (background.sizeDelta.y / 2));
            // 更新输入方向
            inputDirection = touchPosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 重置摇杆位置和输入方向
        handle.anchoredPosition = Vector2.zero;
        inputDirection = Vector2.zero;
    }

    // 返回输入方向
    public Vector2 GetInputDirection()
    {
        return inputDirection;
    }
}
