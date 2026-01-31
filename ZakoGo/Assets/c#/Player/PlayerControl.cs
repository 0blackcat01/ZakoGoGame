using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;


public class PlayerControl : MonoBehaviour
{
    public GameObject JoyStick;
    public float moveSpeed = 5f; // 角色的移动速度
    public float jumpForce = 10f; // 跳跃的力量
    public float crouchSpeed = 2.5f; // 蹲下时的移动速度
    public Transform groundCheck; // 用于检测是否在地面上
    public LayerMask groundLayer; // 地面层
    public bool isCrouching = false; // 是否在蹲下状态

    private Rigidbody2D rb;
    private AudioSource audio0;
    private BoxCollider2D bc2;
    public bool isGrounded = true;
    public float groundCheckDistance = 0.1f; // 检测地面的距离 
    private Animator anim;

    public GameObject Gun;
    private Vector3 originalGunPosition; // 记录枪的原始位置
    private Vector3 originalGunPosition0; // 记录枪的原始位置
    private Vector3 crouchGunPosition; // 蹲下时枪的位置
    public Vector3 GunPosAdd; // 枪在蹲下时的位置偏移
    private Sprite BulletSprite;

    public TextMeshProUGUI PlayerNameTxt;

    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        bc2 = GetComponent<BoxCollider2D>();
        audio0 = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();

        

        RefreshGunNum();
        BulletNumTxt.text = GameNum.BulletNum.ToString();
        TolBulletNumTxt.text = GameNum.TolBulletNum.ToString();
        originalGunPosition = Gun.transform.position; // 获取枪的原始位置
        PlayerNameTxt.text = GameNum.PlayerName.ToString();

        GameNum.CanMoveBagItems = true;

        Checkplatform();
    }
    public void RefreshGunNum()
    {
        if(MyGunList != null)
        {
            ShakeTime = MyGunList.gunList[0].ScreenShakeTime;
            ShakePower  = MyGunList.gunList[0].ScreenShakePower;
            fireRate = MyGunList.gunList[0].FireRate;
            Gun.GetComponent<SpriteRenderer>().sprite = MyGunList.gunList[0].GunChangeBulletImg;
            Gun.GetComponent<GunControl>().GunSprites[0] = MyGunList.gunList[0].GunChangeBulletImg;
            Gun.GetComponent<GunControl>().GunSprites[1] = MyGunList.gunList[0].GunChangeBulletImg;
            Gun.GetComponent<GunControl>().Rounds = MyGunList.gunList[0].BulletBoxNum;
            Gun.transform.position += MyGunList.gunList[0].GunAddPos;
            BulletSprite = MyGunList.gunList[0].BulletImg;
            GameNum.TolBulletNum = 0;
            GameNum.BulletNum = 0;
            gameObject.GetComponent<AudioSource>().clip = MyGunList.gunList[0].clip01;
            IsBreak_audio = MyGunList.gunList[0].Isbreak_audio;
            BulletDieTime = MyGunList.gunList[0].BulletDieTime;


        }
    }

    void Update()
    {
        if (GameNum.IsOpenUI) return;
        CheckHeadWall();
        Move();
        Jump();
        Crouch();
        UpdateAnimation();
        RotateGunWithMouse();
        //Fire();
        ChangeNoGun();
    }
    private bool IsAndroid = false;
    void Checkplatform()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            JoyStick.gameObject.SetActive(true);
            Debug.Log("当前平台是安卓端");
            // 在这里添加安卓平台的相关代码
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer ||
                 Application.platform == RuntimePlatform.WindowsEditor ||
                 Application.platform == RuntimePlatform.OSXPlayer ||
                 Application.platform == RuntimePlatform.OSXEditor)
        {

            Debug.Log("当前平台是电脑端");
            // 在这里添加电脑端的相关代码
        }

    }
    #region 移动
    private void Move()
    {
        float moveInput = Input.GetAxis("Horizontal"); // 获取水平输入
        //左走-1

        float speed = isCrouching ? crouchSpeed : moveSpeed; // 根据状态决定移动速度
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y); // 移动角色
    }
    #endregion

    private bool Isjump =  false;
    private void UpdateAnimation()
    {
        // 更新动画参数
        anim.SetBool("Walk", rb.velocity.x != 0 && isGrounded && !isCrouching);
        if (!isGrounded && !Isjump)
        {
            Isjump = true;
            anim.SetTrigger("Jump");
        }
        else if(isGrounded)
        {
            Isjump = false;
        }
        
        anim.SetBool("Hunker", isCrouching);

        // 检测是否在蹲走状态
        bool isCrouchingWalk = rb.velocity.x != 0 && isCrouching;
        anim.SetBool("HunkerWalk", isCrouchingWalk); // 设置蹲走动画参数
    }
    #region 跳跃
    private void Jump()
    {
        if (!IsHeadPlatform)
        {
            if (IsHeadWall) return;
        }
        
        // 从角色的底部发射一条射线，检测是否与地面碰撞
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        // 可视化射线
        Color rayColor = isGrounded ? Color.green : Color.red; // 根据是否在地面上设置射线颜色
        Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, rayColor);
        if (isGrounded && Input.GetButtonDown("Jump") && !isCrouching) // 当在地面上并按下跳跃键
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // 向上施加跳跃力
        }
    }
    #endregion
    public bool IsHeadWall = false;
    public bool IsHeadPlatform = false;
    private bool HeadWallOnce = false;  
    public void CheckHeadWall()
    {
        // 从角色的顶部发射一条射线，检测是否与地面碰撞
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.up, groundCheckDistance, groundLayer);
        IsHeadWall = hit;
        if(hit.collider != null)
        {
            IsHeadPlatform = hit.collider.CompareTag("platform");
        }
        else
        {
            IsHeadPlatform = false;
        }
        
        // 可视化射线
        Color rayColor = IsHeadWall ? Color.green : Color.red; // 根据是否在地面上设置射线颜色
        Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, rayColor);
        
        if (IsHeadWall && !HeadWallOnce)
        {
            HeadWallOnce = true;
        }
        if (isCrouching)
        {

            if (!Input.GetButton("Crouch") && HeadWallOnce && !IsHeadWall)
            {
                
                isCrouching = false;
                bc2.offset = new Vector2(0, 0);
                bc2.size = new Vector2(bc2.size.x, 2.875f);
                HeadWallOnce = false ;
                Gun.transform.position = new Vector3(gameObject.transform.position.x, originalGunPosition.y, originalGunPosition.z);
            }

        }
    }
    private void Crouch()
    {
        if (!isGrounded) return;
        if (IsHeadPlatform) return;

        if (Input.GetButtonDown("Crouch")) // 按下蹲下键
        {
            
            crouchGunPosition = gameObject.transform.position - GunPosAdd; // 计算蹲下时枪的位置
            isCrouching = true;
            bc2.offset = new Vector2(0, -0.55f);
            bc2.size = new Vector2(bc2.size.x, 1.8f);
            if (IsHasGun)
            {
                StartCoroutine(MoveGunToPosition(crouchGunPosition)); // 开始协程移动枪
            }

            // 你可以在这里添加改变角色形状的代码
        }
        else if (Input.GetButtonUp("Crouch")) // 松开蹲下键
        {
            if (IsHeadWall) return;
            isCrouching = false;
            bc2.offset = new Vector2(0, 0);
            bc2.size = new Vector2(bc2.size.x, 2.875f);
            //Gun.transform.position = new Vector3(gameObject.transform.position.x, originalGunPosition.y, originalGunPosition.z);
            if (IsHasGun)
            {
                StartCoroutine(MoveGunToPosition(gameObject.transform.position - GunPosAdd/2)); // 开始协程移动回原位置
            }
            
        }
    }
    private IEnumerator MoveGunToPosition(Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        float duration = 0.25f; // 移动所需的时间，可以根据需要调整
        Vector3 startingPosition = Gun.transform.position;
        if (isCrouching)
        {
            yield return new WaitForSeconds(0.1f);
        }
        

        while (elapsedTime < duration)
        {
            // 只在Y轴上进行插值，保持X轴位置不变
            Gun.transform.position = new Vector3(gameObject.transform.position.x,
                                                   Mathf.Lerp(startingPosition.y, targetPosition.y, (elapsedTime / duration)),
                                                   startingPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null; // 等待下一帧
        }

        // 确保最终位置是目标位置
        Gun.transform.position = new Vector3(gameObject.transform.position.x, targetPosition.y, startingPosition.z);
    }
    private void RotateGunWithMouse()
    {
        if (!IsHasGun) return;

        // 获取鼠标世界坐标
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane;
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // 计算方向向量
        Vector2 directionToMouse = new Vector2(
            worldMousePosition.x - transform.position.x,
            worldMousePosition.y - transform.position.y
        );

        // 判断角色朝向和鼠标位置
        bool isPlayerFacingLeft = transform.localScale.x > 0;
        bool isMouseInFrontOfPlayer = isPlayerFacingLeft ?
            directionToMouse.x > 0 : directionToMouse.x < 0;

        float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;

        // 处理枪械旋转
        if (isMouseInFrontOfPlayer)
        {
            // 翻转枪械朝向
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            if (transform.localScale.x > 0)
            {
                angle -= 180;
            }
        }
        else if (Mathf.Abs(angle) >= 90)
        {
            // 处理边界情况
            angle -= 180;
        }

        // 应用旋转
        Gun.transform.rotation = Quaternion.Euler(0, 0, angle);
        BulletPos.rotation = Quaternion.Euler(0, 0, angle);
    }
    [Header("发射子弹")]
    public GameObject Pool;
    public float fireRate = 0.5f;  // 发射间隔（秒）
    private float nextFireTime = 0f;  // 下次可以发射的时间
    private bool isFire = false;
    public TextMeshProUGUI BulletNumTxt;
    public TextMeshProUGUI TolBulletNumTxt;
    public Transform BulletPos;
    public GunList MyGunList;
    private float ShakeTime;
    private float ShakePower;
    private bool IsBreak_audio = false;//audio
    private Vector3 GunrotationOffset;
    private float continuousFireTime = 0f; // 持续射击时间
    private float BulletDieTime;
    public void Fire()
    {
        if (!IsHasGun) return;
        // 检测鼠标输入
        if (Input.GetMouseButton(0)) // 按下鼠标左键
        {

            if (GameNum.BulletNum <= 0)
            {
                if (isFire)
                {
                    audio0.loop = false;
                    isFire = false;
                    audio0.Stop();
                }
                return;
            }
            // 增加持续射击时间
            continuousFireTime += Time.deltaTime;

            // 计算当前弹道偏移量（随时间增加，但不超过GunRotation）
            float currentOffsetRatio = Mathf.Clamp01(continuousFireTime / 3f); // 3秒达到最大偏移
            float currentOffset = currentOffsetRatio * MyGunList.gunList[0].GunRotation;
            GunrotationOffset = new Vector3(0, 0, Random.Range(-currentOffset, currentOffset));
            if (Time.time >= nextFireTime)
            {
                // 获取鼠标的世界坐标
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0;  // 确保 z 坐标为 0（假设是 2D 游戏）

                // 检查角色与鼠标位置是否在同一侧
                if (!IsSameSide(mousePosition))
                {

                    FireBullet(mousePosition);

                    GameNum.BulletNum -= 1;
                    BulletNumTxt.text = GameNum.BulletNum.ToString();
                    if (!isFire)
                    {
                        audio0.loop = true;
                        audio0.Play();
                        
                        isFire = true;
                    }
                    nextFireTime = Time.time + fireRate;  // 更新下次发射时间
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (isFire)
            {
                audio0.loop = false;
                if (IsBreak_audio)
                {
                    audio0.Stop();
                }

                isFire = false;
            }
            continuousFireTime = 0f; // 松开鼠标时重置持续射击时间
            GunrotationOffset = Vector3.zero; // 重置弹道偏移

        }
    }

    public void FireBullet(Vector3 mousePosition)
    {
        GameObject bullet = GameObject.FindGameObjectWithTag("Pool").GetComponent<PoolBullet>().GetBullet();
        if (bullet != null)
        {
            // 设置子弹的初始位置
            bullet.transform.position = Gun.transform.position; // 假设玩家的发射位置是玩家物体的位置
            bool isPlayerFacingLeft = gameObject.transform.localScale.x > 0; // 角色朝向左
            bullet.GetComponent<ZakoBullet>().SetPlayerDirection(isPlayerFacingLeft);
            // 应用弹道偏移
            bullet.GetComponent<SpriteRenderer>().sprite = BulletSprite;
            bullet.GetComponent<ZakoBullet>().lifeTime = BulletDieTime;
            bullet.GetComponent<ZakoBullet>().TagKind = 1;
            bullet.transform.rotation = BulletPos.rotation * Quaternion.Euler(GunrotationOffset);

        }
        if (rb.velocity.x > 0f) return;
        Camera.main.GetComponent<CameraMove>().Shake(ShakeTime,ShakePower);
    }
    // 检查鼠标是否与角色在同一侧
    private bool IsSameSide(Vector3 mousePosition)
    {
        // 判断角色的 x 坐标和鼠标的 x 坐标的符号是否相同
        return (mousePosition.x >= transform.position.x && transform.localScale.x > 0) ||
               (mousePosition.x < transform.position.x && transform.localScale.x < 0);
    }
    private bool IsHasGun = true;
    public void ChangeNoGun()
    {
        if (isCrouching || Isjump) return;
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (IsHasGun)
            {
                IsHasGun = false;
                moveSpeed = 5f;
                Gun.GetComponent<GunControl>().IsRenew = false;
                Gun.SetActive(false);
            }
            else
            {
                IsHasGun = true;
                moveSpeed = 6f;
                Gun.SetActive(true);
            }

        }

        
    }
    public Vector2 GetMoveInput()
    {
        // 返回当前移动输入向量
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

}
