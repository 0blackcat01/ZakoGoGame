using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Recipe", menuName = "Recipe/New Recipe")]
[System.Serializable]
public class Recipe : ScriptableObject
{
    public int EnhanceItemID; // 要强化的物品ID
    [Header("设备等级")]
    public int level = 1;
    [System.Serializable]
    public class Ingredient
    {
        public List<Item> item;
        public List<int> amount;
        public int NeedMoney;
        [Header("厨房食物")]
        public int RecipeId;
        public Item Food;
        public float SuccessfulRate;
    }

    public List<Ingredient> ingredients; // 需要的材料和数量
}
