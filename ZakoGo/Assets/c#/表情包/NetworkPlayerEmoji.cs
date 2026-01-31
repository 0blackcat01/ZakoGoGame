using Mirror;
using UnityEngine;

public class NetworkPlayerEmoji : NetworkBehaviour
{
    public Transform emojiDisplayPoint;
    [Header("冷却时间")]
    public float cooldown = 1f;

    private int currentEmojiId = -1;

    private float lastEmojiTime;
    private GameObject currentEmoji;

    public void SeedEmoji(int EmojiID)
    {
        if (!isLocalPlayer) return;
        CmdPlayEmoji(EmojiID, gameObject.GetComponent<NetworkIdentity>().netId);
    }
    [Command]
    public void CmdPlayEmoji(int emojiId, uint playerNetId)
    {
        if (!isServer) return;

        // 冷却检查
        if (Time.time - lastEmojiTime < cooldown) return;

        Debug.Log(emojiId);
        lastEmojiTime = Time.time;
        currentEmojiId = emojiId;
        RpcPlayEmoji(emojiId, playerNetId);
    }


    [ClientRpc]
    private void RpcPlayEmoji(int emojiId,uint playerNetId)
    {
        // 使用 NetworkClient.spawned 替代 NetworkIdentity.spawned
        if (NetworkClient.spawned.TryGetValue(playerNetId, out NetworkIdentity playerIdentity))
        {
            EmojiData emoji0 = EmojiManager.Instance.GetEmoji(emojiId);
            if (emoji0 == null) return;

            NetworkPlayerEmoji playerEmojiController =
                playerIdentity.GetComponent<NetworkPlayerEmoji>();

            Transform displayPoint = playerEmojiController != null &&
                                  playerEmojiController.emojiDisplayPoint != null ?
                playerEmojiController.emojiDisplayPoint : playerIdentity.transform;
            Vector3 displayPosition = displayPoint.position + emoji0.offset;
            PlayEmojiAtPosition(emoji0, displayPosition, displayPoint);
        }
    }
    private void PlayEmojiAtPosition(EmojiData emojiData, Vector3 Pos,Transform transform)
    {
        if (currentEmoji != null)
        {
            Destroy(currentEmoji);
        }

        currentEmoji = Instantiate(
            emojiData.emojiPrefab,
            Pos,
            Quaternion.identity,
            transform
        );

        Destroy(currentEmoji, emojiData.duration);
    }
}