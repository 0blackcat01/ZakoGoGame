using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EmojiUIShow : MonoBehaviour
{
    // Start is called before the first frame update
    public EmojiDataList MyEmojiDataList;
    private void OnEnable()
    {
        UpdateEmoji();
    }
    public void UpdateEmoji()
    {
        for (int i = 0;i < gameObject.transform.childCount;i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        for(int i = 0; i < MyEmojiDataList.datas.Count; i++)
        {
            InstertEmoji(MyEmojiDataList.datas[i]);
        }
    }
    public EmojiGrid EmojiGrid;
    public void InstertEmoji(EmojiData data)
    {
        EmojiGrid emoji0 = Instantiate(EmojiGrid,gameObject.transform);
        emoji0.Emoji = data;
        emoji0.img.sprite = data.Emojisprite;
    }
}
