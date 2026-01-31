using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class CheckVersion : MonoBehaviour
{
    public string version0;
    public TextMeshProUGUI CanOpenTxt;
    public GameObject canvas1;
    public GameObject canvas2;
    // Start is called before the first frame update
    public void Check()
    {
        StartCoroutine(CheakIt());
    }
    [System.Serializable]
    public class CheckPost
    {
        public string version;

    }

    IEnumerator CheakIt()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(GameNum.Server_Api+"version"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                CheckPost serverVersion = JsonConvert.DeserializeObject<CheckPost>(www.downloadHandler.text);
                if (serverVersion.version == version0)
                {
                    canvas1.gameObject.SetActive(true);
                    canvas2.gameObject.SetActive(true);
                }
                else
                {
                    CanOpenTxt.gameObject.SetActive(true);
                    Invoke("CloseTxt", 2f);
                }

            }
        }

    }
    public void CloseTxt()
    {
        CanOpenTxt.gameObject.SetActive(false);
    }
}
