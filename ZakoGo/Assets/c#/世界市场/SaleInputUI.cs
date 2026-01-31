using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class SaleInputUI : MonoBehaviour
{
    public Item SaleItem;
    public int id;
    public int index;
    public int Item_Num;
    public string give_name;
    public TMP_InputField ItemNumfield;
    public TMP_InputField ItemInfofield;
    public TMP_InputField ItemPricefield;
    public TextMeshProUGUI TipTxt;
    public void OpenBool()
    {
        GameNum.WorldMarket = true;
    }
    public void EndEdit()
    {
        int price = int.Parse(ItemPricefield.text);
        int num = int.Parse(ItemNumfield.text);
        if (price <= 0)
        {
            ItemPricefield.text = "0";
        }
        if (num > Item_Num)
        {
            ItemNumfield.text = SaleItem.BoxItemNum.ToString();

        }
        if (num < 0)
        {
            ItemNumfield.text = "0";
        }
    }

    public void UpLoadItem()
    {
        
        if (ItemNumfield.text != null && ItemInfofield.text != null && ItemPricefield.text != null)
        {
            if (int.Parse(GameObject.FindGameObjectWithTag("Market").GetComponent<MarketManager>().moneyTxt.text) <
                int.Parse(ItemPricefield.text) * 0.1f)
            {
                TipTxt.text = "手续费不足";
                TipTxt.gameObject.SetActive(true);
                Invoke("StopTip",2f);
                return;
            }

            GameObject.FindGameObjectWithTag("Market").GetComponent<MarketManager>().SellItem0(ItemInfofield.text,
            int.Parse(ItemNumfield.text), id, GameNum.PlayerName, give_name, int.Parse(ItemPricefield.text));
            StartCoroutine(AddMoneyCoroutine(GameNum.PlayerName, -(int)(int.Parse(ItemPricefield.text) * 0.1f)));
            ItemNumfield.text = null;
            ItemInfofield.text = null;
            ItemPricefield.text = null;
            give_name = null;
        }

    }
    public void StopTip()
    {
        TipTxt.gameObject.SetActive(false);
    }
    public void RecvIndex(Item item)
    {
        SaleItem = item;
        id = item.ItemId;
        Item_Num = item.BoxItemNum;
        give_name = item.ItemName;
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
