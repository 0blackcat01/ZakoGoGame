using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmojiGrid : MonoBehaviour
{
    public EmojiData Emoji;
    public Image img;

    private NetworkPlayerEmoji mNetworkPlayerEmoji;

    private void Start()
    {
        // 初始化时缓存UI组件
        mNetworkPlayerEmoji = GetComponentInParent<NetworkPlayerEmoji>();

    }
    public void SeedEmoji0()
    {
        if (Emoji != null)
        {
            mNetworkPlayerEmoji.SeedEmoji(Emoji.emojiId);
        }
    }
}
