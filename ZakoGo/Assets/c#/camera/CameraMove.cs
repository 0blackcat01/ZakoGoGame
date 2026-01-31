using UnityEngine;
using System.Collections;
using Mirror;


public class CameraMove : MonoBehaviour
{
    private Vector3 lastValidPosition;
    private bool isShaking = false;


    [Header("跟随目标")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 followOffset = new Vector2(0f, 0f);

    [Header("移动设置")]
    [SerializeField] private float followSmoothness = 0.2f;
    [SerializeField] private float maxFollowSpeed = 20f;
    [SerializeField] private float positionSnapThreshold = 0.1f; // 位置捕捉阈值

    [Header("边界设置")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 5f;

    [Header("性能优化")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool interpolateMovement = true; // 是否插值移动
    [SerializeField] private float interpolationSpeed = 10f; // 插值速度

    // 内部变量
    private Camera cam;
    private Vector3 currentVelocity;
    private Vector3 targetPosition;
    private Vector3 lastFramePosition;
    private float cameraHalfWidth;
    private float cameraHalfHeight;
    private float lastUpdateTime;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        Application.targetFrameRate = targetFrameRate;
        CalculateCameraBounds();
    }

    private void Start()
    {
        FindPlayerTarget();

        if (target != null)
        {
            Vector3 startPos = CalculateTargetPosition();
            transform.position = new Vector3(startPos.x, startPos.y, -10);
            lastFramePosition = transform.position;
            targetPosition = transform.position;
        }

        lastUpdateTime = Time.time;
    }

    private void FindPlayerTarget()
    {
        if (GameNum.IsSinglePlayer)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
        else
        {
            if (NetworkClient.localPlayer != null)
            {
                target = NetworkClient.localPlayer.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindPlayerTarget();
            if (target == null) return;
        }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        float deltaTime = Time.time - lastUpdateTime;
        lastUpdateTime = Time.time;

        // 如果deltaTime过长，重置以防止跳跃
        if (deltaTime > 0.1f) deltaTime = 0.016f; // 大约60FPS

        // 计算目标位置
        Vector3 desiredPosition = CalculateTargetPosition();

        // 应用边界
        desiredPosition = ApplyCameraBounds(desiredPosition);

        if (interpolateMovement)
        {
            // 使用Lerp进行更平滑的插值
            targetPosition = Vector3.Lerp(
                targetPosition,
                desiredPosition,
                interpolationSpeed * deltaTime
            );

            // 如果非常接近，直接跳转
            if (Vector3.Distance(targetPosition, desiredPosition) < positionSnapThreshold)
            {
                targetPosition = desiredPosition;
            }

            transform.position = new Vector3(targetPosition.x, targetPosition.y, -10);
        }
        else
        {
            // 使用SmoothDamp
            Vector3 newPosition = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref currentVelocity,
                followSmoothness,
                maxFollowSpeed,
                deltaTime
            );

            newPosition.z = -10;
            transform.position = newPosition;
        }

        lastFramePosition = transform.position;
    }

    private Vector3 CalculateTargetPosition()
    {
        if (target == null) return lastFramePosition;
        return target.position + new Vector3(followOffset.x, followOffset.y, 0);
    }

    private Vector3 ApplyCameraBounds(Vector3 position)
    {
        // 计算相机视口边界
        CalculateCameraBounds();

        float clampedX = Mathf.Clamp(position.x, minX + cameraHalfWidth, maxX - cameraHalfWidth);
        float clampedY = Mathf.Clamp(position.y, minY + cameraHalfHeight, maxY - cameraHalfHeight);

        return new Vector3(clampedX, clampedY, position.z);
    }

    private void CalculateCameraBounds()
    {
        if (cam == null) return;

        cameraHalfHeight = cam.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * cam.aspect;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            // 立即跳转到新目标位置
            Vector3 snapPosition = CalculateTargetPosition();
            transform.position = ApplyCameraBounds(snapPosition);
            targetPosition = transform.position;
            lastFramePosition = transform.position;
        }
    }

    public void SnapToTarget()
    {
        if (target != null)
        {
            Vector3 snapPosition = CalculateTargetPosition();
            transform.position = ApplyCameraBounds(snapPosition);
            targetPosition = transform.position;
            lastFramePosition = transform.position;
            currentVelocity = Vector3.zero;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        if (isShaking) yield break;

        isShaking = true;
        float elapsed = 0.0f;
        Vector3 originalPosition = lastValidPosition;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = originalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = lastValidPosition;
        isShaking = false;
    }
}
