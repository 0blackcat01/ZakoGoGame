using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmGrid : MonoBehaviour
{
    public int Index;
    public SpriteRenderer SpriteRenderer;
    public GameObject SeedUI;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision == null) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            
            SeedUI.SetActive(true);
            GameNum.FarmIndex = Index;
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            SeedUI.SetActive(false);
            GameNum.FarmIndex = -1;
        }
    }
}
