using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New ItemLists", menuName = "Item/New ItemLists")]
[System.Serializable]
public class ItemLists : ScriptableObject
{
    public List<ItemList> itemLists = new List<ItemList>();
}
