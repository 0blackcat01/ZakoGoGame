using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EmojiData", menuName = "Game/Emoji Data")]
public class EmojiData : ScriptableObject
{
    public int emojiId;
    public Sprite Emojisprite;
    public GameObject emojiPrefab; // 3D表情特效
    public float duration = 3f;   // 显示持续时间
    public Vector3 offset = new Vector3(0, 2f, 0); // 头顶偏移
}
