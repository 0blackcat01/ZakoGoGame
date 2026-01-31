using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolBullet : MonoBehaviour
{
    public GameObject bulletPrefab;  // 子弹预设体
    public int poolSize = 20;       // 子弹池的大小

    private Queue<GameObject> bulletPool;  // 子弹池

    void Start()
    {
        // 初始化子弹池
        bulletPool = new Queue<GameObject>();

        // 创建子弹池，初始化预设的子弹对象
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab,gameObject.transform);
            bullet.SetActive(false);  // 将所有子弹初始为不激活
            bulletPool.Enqueue(bullet);  // 将子弹放入池中
        }
    }

    // 获取一个子弹对象
    public GameObject GetBullet()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();  // 从队列中取出一个子弹
            bullet.SetActive(true);  // 激活子弹
            return bullet;
        }
        else
        {
            // 如果池中没有可用的子弹，返回空值
            Debug.LogWarning("Bullet pool is empty!");
            return null;
        }
    }

    // 将子弹放回池中
    public void ReturnBullet(GameObject bullet)
    {
        bullet.transform.rotation = Quaternion.identity;
        bullet.SetActive(false);  // 禁用子弹
        bulletPool.Enqueue(bullet);  // 将子弹放回池中
    }
}
