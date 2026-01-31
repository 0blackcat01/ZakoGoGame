using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New KitchenRecipe", menuName = "Recipe/New KitchenRecipe")]
[System.Serializable]
public class KitchenRecipe : ScriptableObject
{
    public int recipeId;
    public Item Food;
    public int requiredKitchenLevel; // 解锁需要的厨房等级
    public float cookingTime; // 烹饪时间(秒)
    public List<IngredientRequirement> ingredients;
}
[System.Serializable]
public class IngredientRequirement
{
    public Item Item;
    public int amount;
}
