using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New RecipeList", menuName = "Recipe/New RecipeList")]
public class RecipeList : ScriptableObject
{
    public List<Recipe> recipes = new List<Recipe>();
}
