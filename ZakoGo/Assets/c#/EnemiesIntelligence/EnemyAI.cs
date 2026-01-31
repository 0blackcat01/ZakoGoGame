using UnityEngine;
using System.Collections.Generic;
using Mirror.BouncyCastle.Asn1.Cms;
using Mirror;

public abstract class EnemyAI : NetworkBehaviour
{
    [Header("AI Settings")]
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float attackRange = 5f;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float rotationSpeed = 5f;
    [SerializeField] protected float idleTime = 2f;
    [SerializeField] protected float PatrolTime = 5f;
    [SerializeField] private float patrolChangeInterval = 3f; // 方向改变间隔
    [SerializeField] private float patrolRange = 5f;          // 巡逻范围限制
    [SerializeField] protected int AttackNum = 0;
    [SerializeField] protected float DenfenseNum = 0;
    protected float attackCooldown = 0.5f;   // 攻击冷却
    private Vector3 patrolStartPosition;
    private float nextDirectionChangeTime;
    private Vector3 currentPatrolDirection;

    [Header("Detection Settings")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private Vector2 raycastOffset = new Vector2(0, 0.5f);

    protected Transform player;
    protected EnemyState currentState;
    public float stateTimer;
    protected readonly List<Transform> detectedPlayers = new List<Transform>();
    private Animator anim;
    public Transform Gun;
    protected AudioSource audio0;

    [SerializeField] protected Character CharacterSetting;
    [SerializeField] protected CharacterNum m_CharacterNum;


    protected enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Patrol,
        Retreat,
        Special
    }

    #region Unity 生命周期方法

    protected virtual void Awake()
    {

        currentState = EnemyState.Idle;
        stateTimer = idleTime;
        // 初始化检测碰撞体
        anim = GetComponent<Animator>();
        patrolStartPosition = transform.position;
        audio0 = gameObject.GetComponent<AudioSource>(); 
        if(CharacterSetting != null)
        {
            moveSpeed = CharacterSetting.WalkSpeed;
            AttackNum = CharacterSetting.Attack;
            DenfenseNum = CharacterSetting.Denfense;
            attackCooldown = CharacterSetting.FireRate;
        }
    }

    protected virtual void Update()
    {
        if (!isServer) return;
        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdleState();
                break;
            case EnemyState.Chase:
                UpdateChaseState();
                break;
            case EnemyState.Attack:
                UpdateAttackState();
                break;
            case EnemyState.Patrol:
                UpdatePatrolState();
                break;
            case EnemyState.Retreat:
                UpdateRetreatState();
                break;
            case EnemyState.Special:
                UpdateSpecialState();
                break;
        }

        stateTimer -= UnityEngine.Time.deltaTime;
    }
    public void AddPlayer(GameObject other)
    {
        if (detectedPlayers.Contains(other.transform)) return;
        if(!HasLineOfSight(other.transform)) return;
        detectedPlayers.Add(other.transform);
        player = other.transform;
        Debug.Log($"玩家进入范围: {other.name}");
    }
    public void RemovePlayer(GameObject other)
    {
        if (!detectedPlayers.Contains(other.transform)) return;
        detectedPlayers.Remove(other.transform);
        if(detectedPlayers.Count != 0)
        {
            int ran_num = Random.Range(0, detectedPlayers.Count);
            player = detectedPlayers[ran_num];
        }
        Debug.Log($"玩家离开范围: {other.name}");
    }

    #endregion

    #region 状态机方法

    protected virtual void UpdateIdleState()
    {
        if (CanSeePlayer())
        {
            SwitchState(EnemyState.Chase);
            return;
        }

        if (stateTimer <= 0)
        {
            SwitchState(EnemyState.Patrol);
        }
        ResetGunRotation();
    }

    protected virtual void UpdateChaseState()
    {
        if(CheckRetreat())
        {
            SwitchState(EnemyState.Retreat);
            return;
        }
        if (!CanSeePlayer())
        {
            SwitchState(EnemyState.Idle);
            return;
        }

        if (IsPlayerInAttackRange())
        {
            SwitchState(EnemyState.Attack);
            return;
        }
        MoveTowardsPlayer();
        
    }
    protected virtual void UpdateRetreatState()
    {
        
        if (!CanSeePlayer())
        {
            SwitchState(EnemyState.Idle);
            return;
        }
    }

    protected virtual void UpdateAttackState()
    {
        if (CheckRetreat())
        {
            SwitchState(EnemyState.Retreat);
            return;
        }

        if (!IsPlayerInAttackRange())
        {
            SwitchState(EnemyState.Chase);
            return;
        }
        FacePlayer();



    }
    protected virtual bool CheckRetreat()
    {

        return gameObject.GetComponent<CharacterNum>().Hp < 20;
    }

    protected virtual void UpdatePatrolState()
    {
        if (CanSeePlayer())
        {
            SwitchState(EnemyState.Chase);
            return;
        }

        if (stateTimer <= 0)
        {
            SwitchState(EnemyState.Idle);
        }
        ResetGunRotation();
        PatrolRandom();

    }

    protected virtual void UpdateSpecialState()
    {
        // 子类实现特殊状态
    }

    protected void SwitchState(EnemyState newState)
    {
        OnStateExit(currentState);
        currentState = newState;
        OnStateEnter(newState);
    }

    protected virtual void OnStateEnter(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                stateTimer = idleTime;
                break;
            case EnemyState.Patrol:
                stateTimer = PatrolTime;
                anim.SetBool("Walk", true);
                break;
            case EnemyState.Chase:
                anim.SetBool("Walk",true);
                break;
            case EnemyState.Retreat:
                anim.SetBool("Walk", true);
                break;
        }
    }

    protected virtual void OnStateExit(EnemyState state)
    {
        anim.SetBool("Walk", false);
        // 状态退出时的清理逻辑
    }

    #endregion

    #region 检测与移动方法

    protected bool CanSeePlayer()
    {
        return detectedPlayers.Count > 0;
    }
    
    protected bool IsPlayerInAttackRange()
    {
        if (GetVisibleTarget() != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            return distance <= attackRange;
        }
        return false;
    }

    protected Transform GetVisibleTarget()
    {
        foreach (Transform player in detectedPlayers)
        {
            if (HasLineOfSight(player))
            {
                return player;
            }
        }
        return null;
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector2 origin = (Vector2)transform.position + raycastOffset;
        Vector2 direction = (Vector2)target.position - origin;
        float distance = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction.normalized,
            distance,
            obstacleLayers);
        Debug.DrawRay(origin, direction, hit.collider == null ? Color.blue : Color.red, 0.1f);

        return hit.collider == null;
    }
    protected void PatrolRandom()
    {
        // 检查是否需要改变方向
        if (UnityEngine.Time.time >= nextDirectionChangeTime ||
            Vector3.Distance(transform.position, patrolStartPosition) > patrolRange)
        {
            SetRandomPatrolDirection();
        }
        if (IsFacingWall())
        {
            Debug.Log("Wall");
            SetRandomPatrolDirection();
        }
        // 移动
        transform.position += currentPatrolDirection * moveSpeed * UnityEngine.Time.deltaTime;

    }
    protected void MoveTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * UnityEngine.Time.deltaTime;
        FaceDirection(direction);
    }
    protected void BackTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += -direction * moveSpeed * UnityEngine.Time.deltaTime;
        FaceDirection(-direction);
    }
    [Header("瞄准限制")]
    [SerializeField] private Vector3 gunRotationOffset = new Vector3(0, 0, 0); // 旋转偏移
    protected void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        FaceDirection(direction);
        // 计算目标旋转角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 应用旋转偏移
        Quaternion targetRotation = Quaternion.Euler(gunRotationOffset.x, gunRotationOffset.y, angle + gunRotationOffset.z);

        // 平滑旋转
        Gun.rotation = Quaternion.Slerp(Gun.rotation, targetRotation, rotationSpeed * UnityEngine.Time.deltaTime);
        FlipGunSprite(direction.x);
    }
    protected void ResetGunRotation()
    {
        
        Vector3 direction = new Vector3(0,0,0);
        if (transform.localScale.x > 0)
        {
            direction = new Vector3(-1, 0, 0);
        }
        else
        {
            direction = new Vector3(1, 0, 0);
        }
        // 计算目标旋转角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 应用旋转偏移
        Quaternion targetRotation = Quaternion.Euler(gunRotationOffset.x, gunRotationOffset.y, angle + gunRotationOffset.z);

        // 平滑旋转
        Gun.rotation = Quaternion.Slerp(Gun.rotation, targetRotation, rotationSpeed * UnityEngine.Time.deltaTime);
        FlipGunSprite(direction.x);
    }
    private void FlipGunSprite(float xDirection)
    {
        if (xDirection < 0)
        {
            if(Gun.localScale.x > 0)
            {
                Gun.localScale = new Vector3(-Gun.localScale.x, -Mathf.Abs(Gun.localScale.y), Gun.localScale.z);
            }
            // 玩家在左侧，翻转枪械
            
        }
        else
        {
            if(Gun.localScale.x < 0)
            {
                Gun.localScale = new Vector3(-Gun.localScale.x, Mathf.Abs(Gun.localScale.y), Gun.localScale.z);
            }
            // 玩家在右侧，正常朝向
            
        }
    }


    protected void FaceDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            // 2D游戏只需要处理左右翻转
            float scaleX = -Mathf.Sign(direction.x);

            // 保持当前Y轴和Z轴缩放不变
            Vector3 newScale = transform.localScale;
            newScale.x = Mathf.Abs(newScale.x) * scaleX;
            transform.localScale = newScale;
        }
    }
    private void SetRandomPatrolDirection()
    {
        // 随机选择方向 (-1 或 1)
        currentPatrolDirection = new Vector3(Random.value < 0.5f ? -1 : 1, 0, 0);
        FaceDirection(currentPatrolDirection);
        // 重置计时器
        nextDirectionChangeTime = UnityEngine.Time.time + patrolChangeInterval;

        // 记录起始位置
        patrolStartPosition = transform.position;
    }
    [Header("墙壁检测设置")]
    [SerializeField] private float wallCheckDistance = 0.5f; // 墙壁检测距离
    [SerializeField] private LayerMask wallLayer;            // 墙壁层级
    [SerializeField] private Vector2 wallCheckOffset = new Vector2(0, 0.5f); // 检测点偏移
    private bool IsFacingWall()
    {
        // 计算检测起点（考虑偏移）
        Vector2 checkPosition = (Vector2)transform.position +
                               new Vector2(wallCheckOffset.x * Mathf.Sign(transform.localScale.x),
                                           wallCheckOffset.y);

        // 发射射线检测墙壁
        RaycastHit2D hit = Physics2D.Raycast(
            checkPosition,
            currentPatrolDirection,
            wallCheckDistance,
            wallLayer);

        // 可视化调试
        Debug.DrawRay(checkPosition, currentPatrolDirection * wallCheckDistance,
                     hit.collider != null ? Color.red : Color.green);

        return hit.collider != null;
    }
    #endregion

}