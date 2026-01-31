using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CreatName : MonoBehaviour
{
    // Start is called before the first frame update
    const string PlayerName_DATA_KEY = "PlayerName";
    const string PlayerName_FILE_NAME = "PlayerName.game";
    public GameObject InputUI;
    public TMP_InputField InputTxt;

    private static CreatName instance; // 单例实例
    /*
    void Awake()
    {
        // 实现单例模式
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // 如果已存在实例，销毁新创建的
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // 标记为不销毁

        // 其他初始化代码...
    }
    */

    public void OnEnable()
    {
        var saveData = SaveSystem.LoadFromJson<SaveData>(PlayerName_FILE_NAME);
        if (saveData != null)
        {
            LoadFromJson();
            InputUI.SetActive(false);
        }
        else
        {
            InputUI.SetActive(true);
        }
        CheckMyMoeny0();

    }
    void SaveByJson()
    {
        SaveSystem.SaveByJson(PlayerName_DATA_KEY + ".game", SavingData());

    }
    void LoadFromJson()
    {

        var saveData = SaveSystem.LoadFromJson<SaveData>(PlayerName_FILE_NAME);
        LoadData(saveData);
    }

    [System.Serializable]
    class SaveData
    {
        public string Name;
    }
    SaveData SavingData()
    {
        var data = new SaveData();

        data.Name = InputTxt.text;
        return data;
    }
    void LoadData(SaveData data)
    {
        GameNum.PlayerName = data.Name;
    }
    private string Name;
    public void EndName()
    {
        if (InputTxt.text != null)
        {
            StartCoroutine(CreateNameToCloud(InputTxt.text));

        }
    }
    [System.Serializable]
    public class PlayerData
    {
        public string UserName;
    }
    IEnumerator CreateNameToCloud(string PlayerName)
    {
        PlayerData playerData = new PlayerData { UserName = PlayerName };
        string jsonData = JsonUtility.ToJson(playerData); // 使用 JsonUtility 生成 JSON
        using (UnityWebRequest www = new UnityWebRequest(GameNum.Server_Api+"register", "POST"))
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
                Debug.Log("Create Successfully!"); // 输出成功信息
                SaveByJson();
                GameNum.PlayerName = PlayerName; // 直接使用 PlayerName

                InputUI.SetActive(false);
            }
        }
    }
    public void CheckMyMoeny0()
    {
        StartCoroutine(CheckMyMoney());
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
        using (UnityWebRequest www = new UnityWebRequest(GameNum.Server_Api + "check-money", "POST"))
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
    public void ChangeName()
    {
        GameNum.PlayerName = "BlackCat";
    }
    
}
