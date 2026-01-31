using Mirror;
using Mirror.BouncyCastle.Utilities.IO.Pem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ZakoBullet : MonoBehaviour
{
    public float speed = 10f;  // 子弹速度
    public float lifeTime = 2f;  // 子弹生命周期（秒）
    public float lifeTimer;
    public int HurtNum = 10;
    public int TagKind = -1; //0敌人攻击玩家 1玩家攻击敌人
    private Vector3 direction;  // 子弹的方向

    void Update()
    {
        // 移动子弹
        transform.Translate(direction * speed * Time.deltaTime);
    }
    // 启动计时器，当时间结束后销毁子弹
    public void StartLifeTimer()
    {
        lifeTimer = lifeTime;  // 初始化计时器
        StartCoroutine(LifeTimerCoroutine());  // 启动协程
    }
    // 设置子弹发射的方向
    public void SetDirection(bool IsLeft)
    {
        if (IsLeft)
        {
            if (transform.rotation.z <= -90)
            {
                direction = -transform.right;
            }
            else
            {
                direction = transform.right;
            }
            gameObject.transform.localScale = new Vector3(-1, 1, 1);
            
        }
        else
        {
            direction = transform.right;
            gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
        // 计算方向：目标位置 - 当前子弹位置

        //direction = (targetPosition - transform.position).normalized;  // 标准化方向向量 Vector3 targetPosition
        StartLifeTimer();  // 启动生命周期计时器
    }
    public void SetPlayerDirection(bool IsLeft)
    {
        if (IsLeft)
        {
            direction = -transform.right;
            gameObject.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            direction = transform.right;
            gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
        // 计算方向：目标位置 - 当前子弹位置

        //direction = (targetPosition - transform.position).normalized;  // 标准化方向向量 Vector3 targetPosition
        StartLifeTimer();  // 启动生命周期计时器
    }
    // 协程：等待lifeTime时间后将子弹回收
    private IEnumerator LifeTimerCoroutine()
    {
        yield return new WaitForSeconds(lifeTimer);
        GameObject.FindGameObjectWithTag("Pool").GetComponent<PoolBullet>().ReturnBullet(gameObject);  // 将子弹回收回池
    }

    // 当子弹碰到其他物体时返回池中
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (TagKind == 0 && collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<CharacterNum>().Hurt(HurtNum, 0, 0, 0);
                GameObject.FindGameObjectWithTag("Pool").GetComponent<PoolBullet>().ReturnBullet(gameObject);
            }
            else if (TagKind == 1 && collision.gameObject.CompareTag("Enemy"))
            {
                collision.gameObject.GetComponent<CharacterNum>().Hurt(HurtNum, 0, 0, 0);
                GameObject.FindGameObjectWithTag("Pool").GetComponent<PoolBullet>().ReturnBullet(gameObject);
            }
            else if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Enemy"))
            {
                GameObject.FindGameObjectWithTag("Pool").GetComponent<PoolBullet>().ReturnBullet(gameObject);
            }
            else if(TagKind == -1)
            {
                GameObject.FindGameObjectWithTag("Pool").GetComponent<PoolBullet>().ReturnBullet(gameObject);
            }

        }
    }


}
