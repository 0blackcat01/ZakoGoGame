using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GunControl : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Sprite> GunSprites;
    public TextMeshProUGUI TolBulletNumTxt;
    public TextMeshProUGUI BulletNumTxt;
    public int Rounds;

    private float lastInputTime;
    public float inputCooldown = 1f;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && Time.time > lastInputTime + inputCooldown)
        {
            Debug.Log("!");
            lastInputTime = Time.time;
            if (GameNum.TolBulletNum <= 0) return;
            if (IsRenew) return;
            RenewBullet();
        }
    }
    public void RenewBullet()
    {
        IsRenew = true;
        //gameObject.GetComponent<SpriteRenderer>().sprite = GunSprites[1];
        StartCoroutine(DropAndFade());


    }
    public GameObject magazinePrefab; // 弹夹预制体
    public float dropDuration = 1f; // 掉落持续时间
    public float disappearDuration = 1f; // 消失持续时间
    public Vector3 offset;
    public bool IsRenew = false;

    private IEnumerator DropAndFade()
    {
        // 创建弹夹实例
        GameObject magazineInstance = Instantiate(magazinePrefab, gameObject.transform.position, Quaternion.identity);

        // 计算掉落的起始位置
        Vector3 startPosition = magazineInstance.transform.position;

        // 掉落动画
        float elapsedTime = 0f;
        while (elapsedTime < dropDuration)
        {
            float t = elapsedTime / dropDuration;
            magazineInstance.transform.position = Vector3.Lerp(startPosition, startPosition + offset, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保弹夹最终位置为目标位置
        magazineInstance.transform.position = startPosition + offset;

        // 开始逐渐消失
        Renderer magazineRenderer = magazineInstance.GetComponent<Renderer>();
        Color initialColor = magazineRenderer.material.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0); // 目标颜色为完全透明

        elapsedTime = 0f;
        while (elapsedTime < disappearDuration)
        {
            magazineRenderer.material.color = Color.Lerp(initialColor, targetColor, elapsedTime / disappearDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保颜色最终为目标颜色
        magazineRenderer.material.color = targetColor;
        if (GameNum.TolBulletNum >= (Rounds - GameNum.BulletNum))
        {
            GameNum.TolBulletNum -= (Rounds - GameNum.BulletNum);
            GameNum.BulletNum = Rounds;
        }
        else
        {
            GameNum.BulletNum = GameNum.TolBulletNum;
            GameNum.TolBulletNum = 0;
            
        }

        BulletNumTxt.text = GameNum.BulletNum.ToString();
        TolBulletNumTxt.text = GameNum.TolBulletNum.ToString();
        // 销毁弹夹实例
        IsRenew = false;
        Debug.Log(IsRenew);
        gameObject.GetComponent<SpriteRenderer>().sprite = GunSprites[0];
        Destroy(magazineInstance);
    }
}
