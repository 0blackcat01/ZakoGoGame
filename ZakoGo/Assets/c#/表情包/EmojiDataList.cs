using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EmojiDataList", menuName = "Game/Emoji DataList")]
public class EmojiDataList : ScriptableObject
{
    public List<EmojiData> datas = new List<EmojiData>();
}
