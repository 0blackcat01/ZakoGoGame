using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New MakingRecipe", menuName = "Recipe/New MakingRecipe")]
[System.Serializable]
public class MakingRecipe : ScriptableObject
{
    public Item item;
    public Gun gun;
    public List<Item> items;
    public List<int> amount;
    public int NeedMoney;
}
