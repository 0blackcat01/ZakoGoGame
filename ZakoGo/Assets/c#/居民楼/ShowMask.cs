using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShowMask : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision == null) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!collision.GetComponent<NetPlayerControl>().isLocalPlayer) return;
            gameObject.GetComponent<SpriteRenderer>().color = new Color32(255,255,255,0);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {

        if (other == null) return;
        if (other.gameObject.CompareTag("Player"))
        {
            if (!other.GetComponent<NetPlayerControl>().isLocalPlayer) return;
            gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
        }
    }
}
