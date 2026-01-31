using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New MakingRecipeList", menuName = "Recipe/New MakingRecipeList")]
public class MakingRecipeList : ScriptableObject
{

    public List<MakingRecipe> MakingRecipes = new List<MakingRecipe>();
}
