using UnityEngine;
using System.Collections.Generic;

public class EmojiManager : MonoBehaviour
{
    public static EmojiManager Instance;

    public EmojiDataList MyEmojiDataList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public EmojiData GetEmoji(int emojiId)
    {
        return MyEmojiDataList.datas.Find(e => e.emojiId == emojiId);
    }

}