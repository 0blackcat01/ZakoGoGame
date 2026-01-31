using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BagUIShow : MonoBehaviour
{
    public ItemList MyBag;
    public GunList MyGunList;
    public GameObject GunImg;
    public TextMeshProUGUI ContainsNumTxt;
    private void Awake()
    {
        EventManager.Instance.AddListener(EventName.AddMoney,AddMoney);
    }
    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventName.AddMoney, AddMoney);
    }
    private void OnEnable()
    {
        UpdateBagItem();
        UpdateGun();
    }
    public void UpdateGun()
    {
        GunImg.GetComponent<UnityEngine.UI.Image>().color = new Color32(255,255, 255,255); 
        GunImg.GetComponent<UnityEngine.UI.Image>().sprite = MyGunList.gunList[0].GunImg;
    }
    public void UpdateBagItem()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < MyBag.items.Count; i++)
        {
            InsertItem(MyBag.items[i]);
        }
        UpdateBagCountTxt();
    }
    public BoxGrid boxGrid;

    public void UpdateBagCountTxt()
    {
        ContainsNumTxt.text = MyBag.items.Count + "/" + GameNum.BagCount;
    }
    public void InsertItem(Item item)
    {
        BoxGrid grid = Instantiate(boxGrid, gameObject.transform);
        grid.ItemImage.sprite = item.ItemImg;
        grid.ItemNumTxt.text = item.BagItemNum.ToString();
        grid.ItemNameTxt.text = item.ItemName.ToString();
        grid.ItemValueTxt.text = item.RealMoney.ToString();
        grid.item = item;
        grid.ChangeItemColor(item.ItemValue);
    }
    public void AddMoney(object sender,EventArgs e)
    {
        var data = e as Item_Args;
        StartCoroutine(AddMoneyCoroutine(GameNum.PlayerName,data.AddmoneyNum));
    }
    [System.Serializable]
    public class AddMoneyClass
    {
        public string UserName;
        public int Amount;
    }

    private IEnumerator AddMoneyCoroutine(string name, int num)
    {
        AddMoneyClass addMoneyClass = new AddMoneyClass
        {
            UserName = name,
            Amount = num
        };

        string jsonData = JsonUtility.ToJson(addMoneyClass);
        Debug.Log($"Attempting to add money: {jsonData}");

        using (UnityWebRequest www = new UnityWebRequest($"{GameNum.Server_Api}add-currency", "POST"))
        {
            www.timeout = 10; // 设置超时时间
            www.SetRequestHeader("Content-Type", "application/json");
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            // 更完善的错误处理
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Add money failed: {www.error}\nResponse: {www.downloadHandler?.text}");

                // 可以在这里添加失败后的恢复逻辑
                // 例如: boxGrid.item.BoxItemNum += 1; // 回滚数量
            }
            else
            {
                GameNum.Money += num;
                Debug.Log("Money added successfully!");
                Debug.Log($"Server response: {www.downloadHandler?.text}");
            }
        }
    }

}
