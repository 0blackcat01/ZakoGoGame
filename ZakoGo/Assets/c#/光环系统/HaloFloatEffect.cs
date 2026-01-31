using UnityEngine;

public class HaloFloatEffect : MonoBehaviour
{
    [Header("浮动设置")]
    [SerializeField] private float floatHeight = 0.5f;    // 浮动高度范围
    [SerializeField] private float floatSpeed = 1f;      // 浮动速度
    [SerializeField] private AnimationCurve floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 浮动曲线

    [Header("旋转设置")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 30f;  // 旋转速度（度/秒）

    private Vector3 startPosition;
    private float timer;

    private void Start()
    {
        startPosition = transform.localPosition;
        timer = Random.Range(0f, 2f * Mathf.PI); // 随机初始相位
    }

    private void Update()
    {
        // 更新计时器
        timer += Time.deltaTime * floatSpeed;
        if (timer > 2f * Mathf.PI) timer -= 2f * Mathf.PI;

        // 计算浮动位置
        float sinValue = Mathf.Sin(timer);
        float curveValue = floatCurve.Evaluate((sinValue + 1f) * 0.5f); // 将sin值(-1到1)映射到0-1
        float yOffset = curveValue * floatHeight;

        // 应用位置变化
        transform.localPosition = startPosition + new Vector3(0, yOffset, 0);

        // 旋转效果
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }
}