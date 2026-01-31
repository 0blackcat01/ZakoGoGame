using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadBlock : NetworkBehaviour
{
    [SyncVar] public bool IsHasPreson = false;
    [SyncVar] public bool IsDead = false;
    private BoxCollider2D box2d;
    [SerializeField] private float RefreshTime;

    void Start()
    {
        box2d = GetComponent<BoxCollider2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsDead) return;
        if(collision == null) return;
        if(collision.CompareTag("Bullet")) return;
        if (IsHasPreson) return;
        if(collision.CompareTag("Player"))
        {
            IsHasPreson = true;
            collision.gameObject.GetComponent<CharacterNum>().IsInBlcok = true;
        }
        if (collision.CompareTag("Enemy"))
        {
            IsHasPreson = true;
            collision.gameObject.GetComponent<CharacterNum>().IsInBlcok = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null) return;
        if (collision.CompareTag("Bullet")) return;
        if(collision.gameObject.TryGetComponent(out CharacterNum character))
        {
            if (!character.IsInBlcok) return;
        }

        
        if (collision.CompareTag("Player"))
        {            
            IsHasPreson = false;
            collision.gameObject.GetComponent<CharacterNum>().IsInBlcok = false;
        }
        if (collision.CompareTag("Enemy"))
        {
            IsHasPreson = false;
            collision.gameObject.GetComponent<CharacterNum>().IsInBlcok = false;
        }
    }
    public void RefreshBlock0()
    {
        IsDead = true;
        box2d.enabled = false;
        Invoke("RefreshBlock1", RefreshTime);
    }
    public void RefreshBlock1()
    {
        IsDead = false;
        box2d.enabled = true;
        gameObject.GetComponent<CharacterNum>().ResetHp();
    }
}
