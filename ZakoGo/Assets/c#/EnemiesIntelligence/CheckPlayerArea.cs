using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayerArea : MonoBehaviour
{
    [SerializeField] private CircleCollider2D detectionCollider;
    [SerializeField] private float detectionRadius;
    private void Start()
    {
        if (detectionCollider == null) return;
        detectionCollider = gameObject.GetComponent<CircleCollider2D>();
        detectionCollider.radius = detectionRadius;
        detectionCollider.isTrigger = true;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameObject.transform.parent.GetComponent<EnemyAI>().AddPlayer(other.gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameObject.transform.parent.GetComponent<EnemyAI>().RemovePlayer(other.gameObject);
        }
    }

}

