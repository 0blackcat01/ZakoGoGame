using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using System;

public class MarketManager : MonoBehaviour
{
    // Start is called before the first frame update

    void OnEnable()
    {
        IsCheckMyItems = false;
        FetchItems0();
    }
    public MarketGrid marketGrid;
    public GameObject UI;
    public bool IsCheckMyItems = false;
    public GameObject SaleInput;
    public void FetchItems0()
    {
        IsCheckMyItems = false;
        StartCoroutine(FetchItems());
        Invoke("CheckMyMoeny0", 0.2f);
    }

    public void CheckMyMoeny0()
    {
        StartCoroutine(CheckMyMoney());
    }
    IEnumerator FetchItems()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(GameNum.Server_Api + "items"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("FetchItems");
                List<MarketItem> items = JsonConvert.DeserializeObject<List<MarketItem>>(www.downloadHandler.text);
                PopulateItemList(items);

            }
        }
    }
    [System.Serializable]
    public class FetchMyItemsClass
    {
        public string UserName;

    }
    public void FetchMyItem()
    {
        IsCheckMyItems = true;
        for (int i = 0; i < UI.transform.childCount; i++)
        {
            Destroy(UI.transform.GetChild(i).gameObject);
        }
        StartCoroutine(FetchMyItems(GameNum.PlayerName));
    }
    public void FetchMyItems0(string Name)
    {
        IsCheckMyItems = true;
        for (int i = 0; i < UI.transform.childCount; i++)
        {
            Destroy(UI.transform.GetChild(i).gameObject);
        }
        StartCoroutine(FetchMyItems(Name));
    }
    IEnumerator FetchMyItems(string Name)
    {
        FetchMyItemsClass fetchclass = new FetchMyItemsClass { UserName = Name };

        string jsonData = JsonUtility.ToJson(fetchclass);

        using (UnityWebRequest www = new UnityWebRequest(GameNum.Server_Api+"my-items", "POST"))
        {
            www.SetRequestHeader("Content-Type", "application/json");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();


            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error); // 输出错误信息
            }
            else
            {
                Debug.Log("FetchMyItems");
                List<MarketItem> items = JsonConvert.DeserializeObject<List<MarketItem>>(www.downloadHandler.text);
                PopulateItemList(items);
            }
        }

    }
    [System.Serializable]
    public class CheckMoneyClass
    {
        public string UserName;
    }
    [System.Serializable]
    public class Money
    {
        public int Currency;
    }
    [Header("查看金钱")]
    public TextMeshProUGUI moneyTxt;
    IEnumerator CheckMyMoney()
    {
        Debug.Log(GameNum.PlayerName);
        CheckMoneyClass checkMoneyClass = new CheckMoneyClass { UserName = GameNum.PlayerName };
        string jsonData = JsonUtility.ToJson(checkMoneyClass);
        Debug.Log(jsonData);
        using (UnityWebRequest www = new UnityWebRequest(GameNum.Server_Api+"check-money", "POST"))
        {

            www.SetRequestHeader("Content-Type", "application/json");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                
                Money money = JsonConvert.DeserializeObject<Money>(www.downloadHandler.text);
                GameNum.Money = money.Currency;
                moneyTxt.text = money.Currency.ToString();

            }
        }
    }
    void PopulateItemList(List<MarketItem> items)
    {
        for (int i = 0; i < UI.transform.childCount; i++)
        {
            Destroy(UI.transform.GetChild(i).gameObject);
        }
        foreach (var item in items)
        {
            insertItemToUI(item);
        }
    }

    public void insertItemToUI(MarketItem item)
    {

        MarketGrid grid = Instantiate(marketGrid, UI.transform);
        if (IsCheckMyItems)
        {
            grid.gameObject.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "撤下";
        }
        else
        {
            grid.gameObject.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "购买";
        }
        grid.gridImage.sprite = Find_Item_Img(item.ItemID);
        grid.gridNum.text = "数量: " + item.ItemNum.ToString();
        grid.ItemNum = item.ItemNum;
        grid.PriceTxt.text = "售价: " + item.Price.ToString();
        grid.gridName.text = item.ItemName.ToString();
        grid.ItemID = item.ItemID;
        grid.SalerName.text = "出售人: " + item.UserName;
        grid.SalerID = item.UserID;
        grid.itemInfo.text = item.info;


    }

    [Header("查找仓库")]
    public ItemList MarketBag;


    public Sprite Find_Item_Img(int id)
    {
        Sprite sprite = null;
        for (int i = 0; i < MarketBag.items.Count; i++)
        {
            if (id == MarketBag.items[i].ItemId)
            {
                sprite = MarketBag.items[i].ItemImg;
            }
        }

        return sprite;
    }
    public Item Find_Item(int id)
    {
        Item item = null;
        for (int i = 0; i < MarketBag.items.Count; i++)
        {
            if (id == MarketBag.items[i].ItemId)
            {
                item = MarketBag.items[i];
            }
        }

        return item;
    }

    [System.Serializable]
    public class PurchasePost
    {
        public string UserName;
        public int ItemID;
        public int SellerID;
    }
    [Header("玩家背包")]
    public ItemList MyBox;


    public void PurchaseItem0(string name, int itemID, int salerID, int Itemnum)
    {
        StartCoroutine(PurchaseItem(name, itemID, salerID, Itemnum));
    }
    IEnumerator PurchaseItem(string name, int itemID, int salerID, int Itemnum)
    {
        PurchasePost purchasePost = new PurchasePost { UserName = name, ItemID = itemID, SellerID = salerID };

        string jsonData = JsonUtility.ToJson(purchasePost);
        Debug.Log(jsonData);
        using (UnityWebRequest www = new UnityWebRequest(GameNum.Server_Api+"purchase", "POST"))
        {
            www.SetRequestHeader("Content-Type", "application/json");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();


            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error); // 输出错误信息
            }
            else
            {
                Debug.Log("Purchase successful");
                Item ReItem = Find_Item(itemID);
                if (AddItem(ReItem, Itemnum))
                {
                    StartCoroutine(FetchItems());
                    StartCoroutine(CheckMyMoney());
                    // 刷新商品列表或做其他处理
                }

            }
        }

    }

    public bool AddItem(Item item, int ItemNum)
    {
        if (item.IsStack)
        {
            if (!MyBox.items.Contains(item))
            {
                if (MyBox.items.Count >= GameNum.BoxCount)
                    return false;
                MyBox.items.Add(item);
            }
        }
        else
        {
            if(MyBox.items.Count >= GameNum.BoxCount)
                return false;
            MyBox.items.Add(item);
        }
        item.BoxItemNum += ItemNum;
        GameObject.FindGameObjectWithTag("BoxUI")?.GetComponent<BoxUIShow>().UpdateBoxItem();
        return true;

    }
    public void RedeceItem(int id, int num)
    {
        Item item = MyBox.items.Find(i => i.ItemId == id);
        item.BoxItemNum -= num;
        MyBox.CheckBoxZeroItem();
        GameObject.FindGameObjectWithTag("BoxUI")?.GetComponent<BoxUIShow>().UpdateBoxItem();

    }
    [System.Serializable]
    public class SellItemPost
    {
        public string Info;
        public int ItemNum;
        public int ItemID;
        public string UserName;
        public string ItemName;
        public int Price;
        public int IsItem;

    }
    public void SellItem0(string info, int num, int itemID, string userName, string itemName, int price)
    {
        StartCoroutine(SellItem(info, num, itemID, userName, itemName, price));
    }
    IEnumerator SellItem(string info, int num, int itemID, string userName, string itemName, int price)
    {
        //上架物品

        SellItemPost sellItemPost = new SellItemPost { Info = info, ItemNum = num, ItemID = itemID, UserName = userName, ItemName = itemName, Price = price };
        Debug.Log(sellItemPost.Price);
        string jsonData = JsonUtility.ToJson(sellItemPost);
        using (UnityWebRequest www = new UnityWebRequest(GameNum.Server_Api+"sell", "POST"))
        {
            www.SetRequestHeader("Content-Type", "application/json");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error); // 输出错误信息
            }
            else
            {
                Debug.Log("Sell successful");
                RedeceItem(itemID, num);
                StartCoroutine(FetchMyItems(GameNum.PlayerName));
                // 刷新商品列表或做其他处理
            }
        }

    }
    [System.Serializable]
    public class RemoveItemPost
    {
        public int ItemID;
        public string UserName;


    }
    public void RemoveItem0(int itemID, string userName, int itemNum)
    {
        StartCoroutine(RemoveItem(itemID, userName, itemNum));
    }
    IEnumerator RemoveItem(int itemID, string userName, int itemNum)
    {
        //撤下商品

        RemoveItemPost removeItemPost = new RemoveItemPost { ItemID = itemID, UserName = userName };
        string jsonData = JsonUtility.ToJson(removeItemPost);
        Debug.Log(jsonData);
        using (UnityWebRequest www = new UnityWebRequest(GameNum.Server_Api+"remove-item", "POST"))
        {
            www.SetRequestHeader("Content-Type", "application/json");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error); // 输出错误信息
            }
            else
            {
                for (int i = 0; i < UI.transform.childCount; i++)
                {
                    Destroy(UI.transform.GetChild(i).gameObject);
                }
                Item item = Find_Item(itemID);
                AddItem(item, itemNum);

                StartCoroutine(FetchMyItems(GameNum.PlayerName));
                // 刷新商品列表或做其他处理
            }
        }

    }
    [System.Serializable]
    public class MarketItemList
    {
        public List<MarketItem> MarketItems;
    }
    [System.Serializable]
    public class MarketItem
    {
        public int ItemID;
        public int UserID;
        public string UserName;
        public string ItemName;
        public int Price;
        public string info;
        public int ItemNum;
    }
}
