using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;



public class NetPlayerControl : NetworkBehaviour
{
    public GameObject JoyStick;
    [Header("角色属性")]
    public float moveSpeed = 5f; // 角色的移动速度
    public float jumpForce = 10f; // 跳跃的力量
    public float crouchSpeed = 2.5f; // 蹲下时的移动速度
    public float CritRate = 0; //暴击率
    public float CritNum = 0; //暴伤值

    public Transform groundCheck; // 用于检测是否在地面上
    public LayerMask groundLayer; // 地面层
    public LayerMask JumpLayer; // 跳跃层
    public bool isCrouching = false; // 是否在蹲下状态

    private Rigidbody2D rb;
    private AudioSource audio0;
    private CapsuleCollider2D bc2;
    public bool isGrounded = true;
    public float groundCheckDistance = 0.1f; // 检测地面的距离 
    private Animator anim;

    public GameObject Gun;
    private int GunDamage = 0;
    private Vector3 originalGunPosition; // 记录枪的原始位置
    private Vector3 originalGunPosition0; // 记录枪的原始位置
    private Vector3 crouchGunPosition; // 蹲下时枪的位置
    public Vector3 GunPosAdd; // 枪在蹲下时的位置偏移
    private int BulletSpriteIndex = 0;

    public TextMeshProUGUI PlayerNameTxt;
    [HideInInspector]
    public bool IsOnUI = false;
    
    private CharacterNum MyCharacterNum;

    [SyncVar(hook = nameof(OnGunIndexChanged))]
    private int currentGunId = -1;
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    public string syncPlayerName = "";
    [SerializeField] private GunList AllGunList;

    public GameObject CanvasUI;//信息UI
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isClient) // 如果是客户端而非主机
        {
            // 可以在这里添加一个延迟请求，确保网络已完全建立
            StartCoroutine(RequestResourceSyncAfterDelay());

        }
        if (!isLocalPlayer)
        {
            // 立即隐藏其他玩家的UI
            CanvasUI.gameObject.SetActive(false);
        }
    }
    #region 新客户端名字同步
    private void OnPlayerNameChanged(string oldName, string newName)
    {       
        PlayerNameTxt.text = newName;
    }
    // 当currentGunIndex变化时，所有客户端自动执行
    private void OnGunIndexChanged(int oldIndex, int newIndex)
    {
        if (MyGunList != null)
        {
            var gunData = FromIDFindGun(newIndex);
            Gun.GetComponent<SpriteRenderer>().sprite = gunData.GunChangeBulletImg;

            originalGunPosition = Gun.transform.position; // 获取枪的原始位置
            Gun.transform.position += MyGunList.gunList[0].GunAddPos;

        }
    }
    private IEnumerator RequestResourceSyncAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (isLocalPlayer)
        {
            CmdSetGunIndex(MyGunList.gunList[0].GunID);
            Debug.Log(GameNum.PlayerName);
            CmdSetPlayerName(GameNum.PlayerName);
            CmdRequestResourceSync();
            //CmdGetOtherPlayersData();

        }


    }
    [Command]
    private void CmdGetOtherPlayersData()
    {
        // 服务器收集其他玩家数据
        var allPlayers = FindObjectsOfType<NetPlayerControl>();

        foreach (var otherPlayer in allPlayers)
        {
            if (otherPlayer != this)
            {
                // 告诉新玩家这个玩家的信息
                TargetReceivePlayerInfo(
                    connectionToClient,
                    otherPlayer.netId,
                    otherPlayer.syncPlayerName,
                    otherPlayer.currentGunId
                );
            }
        }
    }


    [TargetRpc]
    private void TargetReceivePlayerInfo(NetworkConnection target, uint playerId, string playerName, int gunIndex)
    {
        var player = FindPlayerByNetIdSimple(playerId);
        if (player != null)
        {
            player.PlayerNameTxt.text = playerName;
            
        }
        if (player.MyGunList != null && gunIndex < player.MyGunList.gunList.Count)
        {
            var gunData = player.MyGunList.gunList[gunIndex];
            player.Gun.GetComponent<SpriteRenderer>().sprite = gunData.GunChangeBulletImg;
        }
    }

    // 通用的查找方法
    private NetPlayerControl FindPlayerByNetIdSimple(uint netId)
    {
        var allPlayers = FindObjectsOfType<NetPlayerControl>();
        foreach (var player in allPlayers)
        {
            if (player.netId == netId)
            {
                return player;
            }
        }
        return null;
    }
    #endregion


    [Command]
    private void CmdRequestResourceSync()
    {
        
        // 服务器收到请求后再次同步
        TolResourcePoint.Instance.SyncResourcesToNewPlayer(connectionToClient);
    }
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Camera.main.GetComponent<CameraMove>().SetTarget(transform);


    }


    void Start()
    {
        // 获取图层ID
        standingLayerID = LayerMask.NameToLayer(standingLayer);
        crouchingLayerID = LayerMask.NameToLayer(crouchingLayer);

        GameNum.IsSinglePlayer = false;
        rb = GetComponent<Rigidbody2D>();
        bc2 = GetComponent<CapsuleCollider2D>();
        audio0 = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();


        moveSpeed = GameNum.MoveSpeed + GameNum.MoveSpeedAddtion;
        jumpForce += GameNum.JumpForceAddtion;

        GameNum.CanMoveBagItems = false;
        MyCharacterNum = gameObject.GetComponent<CharacterNum>();
        RefreshGunNum();
        /*  Test
        NetworkIdentity[] identities = FindObjectsOfType<NetworkIdentity>();
        foreach (var identity in identities)
        {
            Debug.Log($"Scene Object: {identity.name}, AssetId: {identity.netId}");
        }
        */
    }
    public void RefreshGunNum()
    {
        if (MyGunList != null)
        {
            ShakeTime = MyGunList.gunList[0].ScreenShakeTime;
            ShakePower = MyGunList.gunList[0].ScreenShakePower;
            fireRate = MyGunList.gunList[0].FireRate;
            //Gun.GetComponent<SpriteRenderer>().sprite = MyGunList.gunList[0].GunChangeBulletImg;
            Gun.GetComponent<GunControl>().GunSprites[0] = MyGunList.gunList[0].GunChangeBulletImg;
            Gun.GetComponent<GunControl>().GunSprites[1] = MyGunList.gunList[0].GunChangeBulletImg;
            Gun.GetComponent<GunControl>().Rounds = MyGunList.gunList[0].BulletNum;
            CritRate = GameNum.CritRateNum + GameNum.CritRateAddtion + MyGunList.gunList[0].GunCritRate;
            CritNum = MyGunList.gunList[0].GunCritNum;
            BulletSpriteIndex = MyGunList.gunList[0].BulletSpriteIndex;
            //Debug.Log(Gun.GetComponent<GunControl>().Rounds);

            if(MyGunList.gunList[0].Bullet.BagItemNum <= MyGunList.gunList[0].BulletNum)
            {
                GameNum.TolBulletNum = 0;
                GameNum.BulletNum = MyGunList.gunList[0].Bullet.BagItemNum;
            }
            else
            {
                GameNum.TolBulletNum = MyGunList.gunList[0].Bullet.BagItemNum - MyGunList.gunList[0].BulletNum;
                GameNum.BulletNum = MyGunList.gunList[0].BulletNum;
            }

            BulletNumTxt.text = GameNum.BulletNum.ToString();
            TolBulletNumTxt.text = GameNum.TolBulletNum.ToString();
            gameObject.GetComponent<AudioSource>().clip = MyGunList.gunList[0].clip01;
            IsBreak_audio = MyGunList.gunList[0].Isbreak_audio;
            BulletDieTime = MyGunList.gunList[0].BulletDieTime;
            GunDamage = (int)(MyGunList.gunList[0].AttackNum);

        }
    }



    [Command]
    private void CmdSetGunIndex(int newIndex)
    {
        // 服务器修改SyncVar，自动同步到所有客户端
        currentGunId = newIndex;
        if (MyGunList != null)
        {
            var gunData = FromIDFindGun(newIndex);
            Gun.GetComponent<SpriteRenderer>().sprite = gunData.GunChangeBulletImg;

            originalGunPosition = Gun.transform.position; // 获取枪的原始位置
            Gun.transform.position += MyGunList.gunList[0].GunAddPos;

        }
    }
    public Gun FromIDFindGun(int id)
    {
        return AllGunList.gunList.Find(i => i.GunID == id);
    }
    [Command]
    private void CmdSetPlayerName(string Name)
    {
        // 服务器修改SyncVar，自动同步到所有客户端
        syncPlayerName = Name;
        PlayerNameTxt.text = Name;
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        OpenFoodUI();
        OpenEmojiUI();
        OpenAndCloseBag();
        if (IsOnUI) return;
        CheckHeadWall();
        Move();
        Jump();
        Crouch();
        UpdateAnimation();
        RotateGunWithMouse();
        Fire();
        ChangeNoGun();

    }
    #region 系统平台检测
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
    #endregion
    #region 移动
    private void Move()
    {
        float moveInput = Input.GetAxis("Horizontal"); // 获取水平输入
        float speed = isCrouching ? crouchSpeed : moveSpeed; // 根据状态决定移动速度
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y); // 移动角色
    }
    #endregion

    private bool Isjump = false;
    private void UpdateAnimation()
    {
        //if (!isLocalPlayer) return; //不应操作非本地玩家
        // 更新动画参数
        anim.SetBool("Walk", rb.velocity.x != 0 && isGrounded && !isCrouching);
        if (!isGrounded && !Isjump)
        {
            Isjump = true;
            anim.SetTrigger("Jump");
        }
        else if (isGrounded)
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
        if (IsHeadWall) return;
        if(isCrouching) return;
        // 从角色的底部发射一条射线，检测是否与地面碰撞
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        // 可视化射线
        Color rayColor = isGrounded ? Color.green : Color.red; // 根据是否在地面上设置射线颜色
        Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, rayColor);
        RaycastHit2D hit0 = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, JumpLayer);
        if ((isGrounded || hit0) && Input.GetButtonDown("Jump") && !isCrouching) // 当在地面上并按下跳跃键
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // 向上施加跳跃力
        }
    }
    #endregion
    public bool IsHeadWall = false;
    public bool IsFootPlatform = false;
    private bool HeadWallOnce = false;

    public void CheckHeadWall()
    {
        // 从角色的顶部发射一条射线，检测是否与地面碰撞
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D hit0 = Physics2D.Raycast(groundCheck.position, Vector2.up, groundCheckDistance, groundLayer);
        IsHeadWall = hit0;

        if (hit.collider != null)
        {
            IsFootPlatform = hit.collider.CompareTag("platform");
        }
        else
        {
            IsFootPlatform = false;
            if(gameObject.layer != standingLayerID)
            {
                gameObject.layer = standingLayerID;
            }
            
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
                HeadWallOnce = false;
                Gun.transform.position = new Vector3(gameObject.transform.position.x, originalGunPosition.y, originalGunPosition.z);
            }

        }
    }
    [Header("碰撞图层设置")]
    [SerializeField] private string standingLayer = "Player";
    [SerializeField] private string crouchingLayer = "CrouchingPlayer";
    private int standingLayerID;
    private int crouchingLayerID;
    private void Crouch()
    {
        if (!isGrounded) return;

        if (Input.GetButtonDown("Crouch")) // 按下蹲下键
        {

            crouchGunPosition = gameObject.transform.position - GunPosAdd; // 计算蹲下时枪的位置
            if (!isCrouching)
            {
                isCrouching = true;
                bc2.offset = new Vector2(0, -0.55f);
                bc2.size = new Vector2(bc2.size.x, 1.8f);
                if (IsFootPlatform)
                {
                    gameObject.layer = crouchingLayerID;
                }
            }

            if (IsHasGun)
            {
                StartCoroutine(MoveGunToPosition(crouchGunPosition)); // 开始协程移动枪
            }
        }
        else if (Input.GetButtonUp("Crouch")) // 松开蹲下键
        {
            if (IsHeadWall) return;
            if (isCrouching)
            {
                isCrouching = false;
                bc2.offset = new Vector2(0, 0);
                bc2.size = new Vector2(bc2.size.x, 2.875f);
                gameObject.layer = standingLayerID;
            }
  
            //Gun.transform.position = new Vector3(gameObject.transform.position.x, originalGunPosition.y, originalGunPosition.z);
            if (IsHasGun)
            {
                //StartCoroutine(MoveGunToPosition(gameObject.transform.position + new Vector3(0, -0.2f, 0))); // 开始协程移动回原位置
                Gun.transform.position = gameObject.transform.position + new Vector3(0, -0.2f, 0);
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
        if (!isLocalPlayer) return;
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
    #region 开火
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
    private float BulletDieTime = 2f;
    public void Fire()
    {
        if (!IsHasGun) return;
        // 检测鼠标输入
        if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)) // 按下鼠标左键
        {

            if (GameNum.BulletNum <= 0)
            {
                if (isFire)
                {
                    //audio0.loop = false;
                    isFire = false;
                    //audio0.Stop();
                }
                return;
            }
            // 增加持续射击时间
            continuousFireTime += Time.deltaTime;

            // 计算当前弹道偏移量（随时间增加，但不超过GunRotation）
            float currentOffsetRatio = Mathf.Clamp01(continuousFireTime / 3f); // 3秒达到最大偏移
            float currentOffset = currentOffsetRatio * MyGunList.gunList[0].GunRotation;
            GunrotationOffset = new Vector3(0, 0, UnityEngine.Random.Range(-currentOffset, currentOffset));
            if (Time.time >= nextFireTime)
            {
                // 获取鼠标的世界坐标
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0;  // 确保 z 坐标为 0（假设是 2D 游戏）

                // 检查角色与鼠标位置是否在同一侧
                if (!IsSameSide(mousePosition))
                {

                    Cmdshoot(mousePosition,BulletDieTime, BulletPos.rotation,GunrotationOffset,
                        Gun.transform.position, BulletSpriteIndex,MyCharacterNum.IsInBlcok,CritRate,CritNum,
                        NetworkClient.connection.identity.netId);

                    GameNum.BulletNum -= 1;
                    BulletNumTxt.text = GameNum.BulletNum.ToString();
                    audio0.Play();
                    if (!isFire)
                    {
                        //audio0.loop = true;
                        

                        //isFire = true;
                    }
                    nextFireTime = Time.time + fireRate;  // 更新下次发射时间
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (isFire)
            {
                //audio0.loop = false;
                if (IsBreak_audio)
                {
                    //audio0.Stop();
                }

                isFire = false;
            }
            continuousFireTime = 0f; // 松开鼠标时重置持续射击时间
            GunrotationOffset = Vector3.zero; // 重置弹道偏移

        }

    }
    [Command]
    private void Cmdshoot(Vector3 mousePosition,float bulletDieTime,Quaternion rotation,
        Vector3 gunrotationOffset,Vector3 GunPos,int spriteindex,bool IsInBlock,float critRate,float critNum,uint PlayerID)
    {
        GameObject bullet = PoolBulletNet.Instance.GetBullet(BulletType.子弹);
        if (bullet != null)
        {

            bool isPlayerFacingRight = gameObject.transform.localScale.x > 0; // 角色朝向右侧
            bullet.GetComponent<ZakoBulletNet>().SetPlayerDirection(isPlayerFacingRight);       
            
            bullet.GetComponent<ZakoBulletNet>().Initialize(
                    bulletLifetime: BulletDieTime,
                    bulletDamage: GunDamage,
                    playerBullet: true,
                    pelletrotation: rotation * Quaternion.Euler(gunrotationOffset),
                    Pos: GunPos,
                    Spriteindex: spriteindex,
                    Isinblock: IsInBlock,
                    critrate: critRate,
                    critnum: critNum,
                    playerId: PlayerID


            );
            bullet.gameObject.SetActive(true);
            NetworkServer.Spawn(bullet);
        }
    }
    


    #endregion
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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (IsHasGun)
            {
                IsHasGun = false;
                moveSpeed = GameNum.MoveSpeed + GameNum.MoveSpeedAddtion - 1;
                Gun.GetComponent<GunControl>().IsRenew = false;
                Gun.SetActive(false);
            
            }
            else
            {
                IsHasGun = true;
                moveSpeed = GameNum.MoveSpeed + GameNum.MoveSpeedAddtion;
                Gun.SetActive(true);
            }

        }


    }



    #region 物资交互
    [Header("UI组件")]
    public Button SearchButton;
    public ItemList SearchBox;
    public ItemList MyBag;
    public GameObject SearchUIConten;
    public GameObject SearchUI;
    public GameObject CharacterUI;
    public GameObject CharacterUIConten;
    public ItemLists globalResourceList;
    //public List<ItemList> globalResourceList; // 全局资源列表
    public ItemList AllItems;
    public int BoxIndex;
    public TextMeshProUGUI BoxNameTxt;

    // 初始化背包（服务器调用）



    public void OpenSearchButton(int index)
    {
        BoxIndex = index;
        SearchButton.gameObject.SetActive(true);
        SearchButton.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "搜索";
        SearchBox = globalResourceList.itemLists[index];
    }
    [Client]
    public void CloseSearchButton()
    {
        SearchButton.gameObject.SetActive(false);
    }

    public void OpenSearchUI()
    {
        if (SearchUI == null || CharacterUI == null) return;
        SearchUI.gameObject.SetActive(true);
        IsOnUI = true;
        SearchUIConten.GetComponent<SearchShowUI>().UpdateBoxItem(globalResourceList.itemLists[BoxIndex]);
        CharacterUI.gameObject.SetActive(true);
        BoxNameTxt.text = globalResourceList.itemLists[BoxIndex].BoxName;
    }
    public void CloseSearchUI()
    {
        IsOnUI = false;
        DelButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        GameNum.IsOpenUI = false;
        GameNum.CanMoveBagItems = false;
    }



    public void TakeItemFromBox(int itemID,int count,int index)
    {

        if (!isLocalPlayer) return;
        Item item = AllItems.items.Find(i => i.ItemId == itemID);
        if (item == null) return;
        if (index >= globalResourceList.itemLists[BoxIndex].OtherNums.Count) return;
        TolResourcePoint.Instance.CmdUpdateSearchUI(itemID,
            count, index, NetworkClient.connection.identity.netId,BoxIndex);
        
    }
    public void UpdateBagItem(int itemID, int count,uint playerID)
    {
        if (NetworkClient.connection.identity.netId != playerID) return;
        Item item = AllItems.items.Find(i => i.ItemId == itemID);
        if (!MyBag.items.Contains(item))
        {
            MyBag.items.Add(item);
        }
        item.BagItemNum += count;
        CharacterUIConten.GetComponent<NetBagUIShow>().UpdateBagItem();
    }

    
    
    [Header("基础设置")]
    [SerializeField] private Image fillImage0;      // 圆形填充Image组件
    [SerializeField] private Image fillImage1;      // 圆形填充Image组件
    [SerializeField] private List<Sprite> fillSprites;
    [SerializeField] private float fillDuration = 2f; // 完整填充所需时间
    [SerializeField] private float rotationSpeed = 180f; // 旋转速度(度/秒)
    private bool IsEating = false;

    [Header("视觉效果")]
    [SerializeField] private AnimationCurve fillCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private bool clockwise = true; // 顺时针旋转

    private Coroutine loadingCoroutine;
    private bool isLoading = false;
    private int AddHPNum = 0;

    public void EatingStart(int num)
    {
        IsEating = true;
        AddHPNum = num;
        fillImage1.sprite = fillSprites[1];
        StartLoading();
    }
    // 开始加载
    public void StartLoading()
    {
        if (isLoading) return;
        
        fillImage0.transform.position = gameObject.transform.position + new Vector3(0,3f,0);
        // 重置状态
        fillImage1.fillAmount = 0f;
        fillImage0.gameObject.SetActive(true);

        // 启动协程
        loadingCoroutine = StartCoroutine(LoadingAnimation());
        isLoading = true;
    }

    // 停止加载
    public void StopLoading()
    {
        if (!isLoading) return;

        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }
        IsEating = false;
        fillImage1.sprite = fillSprites[0];
        AddHPNum = 0;
        fillImage0.gameObject.SetActive(false);
        isLoading = false;
        IsOnUI = false;
    }

    // 加载动画协程
    private IEnumerator LoadingAnimation()
    {
        float timer = 0f;
        int rotationDirection = clockwise ? -1 : 1;

        while (timer < fillDuration)
        {
            // 更新填充量（应用曲线）
            float progress = timer / fillDuration;
            fillImage1.fillAmount = fillCurve.Evaluate(progress);

            // 旋转效果
            fillImage1.transform.Rotate(0, 0, rotationDirection * rotationSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        // 确保完全填充
        fillImage1.fillAmount = 1f;

        // 加载完成
        OnLoadingComplete();
    }

    // 加载完成回调
    private void OnLoadingComplete()
    {
        Debug.Log("加载完成!");      
        if(IsEating)
        {
            AddHP(AddHPNum);         
        }
        else
        {
            if (IsDigSomeThing)
            {
                if(SignIndex != -1)
                {
                    DropManager.Instance.CmdSignMine(SignIndex);
                }
                SearchButton.gameObject.SetActive(false);
                DropManager.Instance.SpawnDrops(SpawnPos,SometingIndex);
            }
            else
            {
                OpenSearchUI();
            }
        }

        StopLoading();

        // 可以在这里触发事件
        // OnComplete?.Invoke();
    }
    #endregion

    #region 食物面板
    [Header("食物面板")]
    public GameObject FoodUI;
    public void OpenFoodUI()
    {
       if(!isLocalPlayer) return;
       if(Input.GetKeyDown(KeyCode.E))
       {
            if(IsOnUI)
            {
                IsOnUI = false;
                FoodUI.gameObject.SetActive(false);
            }
            else
            {
                IsOnUI = true;
                FoodUI.gameObject.SetActive(true);
                rb.velocity = new Vector2(0, 0); 
                UpdateAnimation();

            }
            
       }
        
    }
    [Command]
    public void AddHP(int num)
    {
        gameObject.GetComponent<CharacterNum>().AddHP(num);
        FoodUI.gameObject.SetActive(false);
    }
    #endregion
    public Vector2 GetMoveInput()
    {
        // 返回当前移动输入向量
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
    public Button DelButton;
    public void RemoveBagitem()
    {
        if (!GameNum.CanMoveBagItems)
        {
            DelButton.GetComponent<Image>().color = new Color32(200,200,200,255);
            GameNum.IsOpenUI = true;
            GameNum.CanMoveBagItems = true;
        }
        else
        {
            DelButton.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            GameNum.IsOpenUI = false;
            GameNum.CanMoveBagItems = false;
        }
        
    }
    #region 表情面板
    [Header("表情面板")]
    public GameObject EmojiUI;
    public void OpenEmojiUI()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (IsOnUI)
            {
                IsOnUI = false;
                EmojiUI.gameObject.SetActive(false);
            }
            else
            {
                IsOnUI = true;
                EmojiUI.gameObject.SetActive(true);
                rb.velocity = new Vector2(0, 0);
                UpdateAnimation();

            }

        }

    }
    #endregion
    #region 打开背包
    [Header("背包退出按钮")]
    public GameObject BagButton;
    public void OpenAndCloseBag()
    {
        if(!Input.GetKeyDown(KeyCode.Tab)) return;
        if (CharacterUI == null) return;
        BagButton.gameObject.SetActive(true);
        IsOnUI = !IsOnUI;
        rb.velocity = new Vector2(0, 0);
        CharacterUI.gameObject.SetActive(IsOnUI);

    }
    public void CloseBag()
    {
        if (CharacterUI == null) return;
        BagButton.gameObject.SetActive(false);
        IsOnUI = false;
        CharacterUI.gameObject.SetActive(false);

    }
    #endregion
    #region 采矿
    private bool IsDigSomeThing = false;
    private int SometingIndex = -1;
    private int SignIndex = -1;
    private Vector3 SpawnPos = new Vector3(0,0,0);
    public void OpenMineButton(int index,Vector3 Pos,int signIndex)
    {
        IsDigSomeThing = true;
        SometingIndex = index;
        SignIndex = signIndex;
        SpawnPos = Pos;
        SearchButton.gameObject.SetActive(true);
        SearchButton.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "开采";

    }
    [Client]
    public void CloseWhichButton()
    {
        IsDigSomeThing = false;
        SometingIndex = -1;
        SignIndex = -1;
        SpawnPos = new Vector3(0, 0, 0);
        SearchButton.gameObject.SetActive(false);
    }
    #endregion
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = GameNum.MoveSpeed + GameNum.MoveSpeedAddtion + speed;
    }
    public void SetDamage(int damage)
    {
        GunDamage = (int)(MyGunList.gunList[0].AttackNum) + damage;
    }
}
