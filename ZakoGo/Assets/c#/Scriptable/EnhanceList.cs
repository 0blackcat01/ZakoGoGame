using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "New EnhanceList", menuName = "EnhanceList/New EnhanceList")]
public class EnhanceList : ScriptableObject
{
    public const int TILE_COUNT = 3;
    public EnhanceTile[] tiles = new EnhanceTile[TILE_COUNT];
    public Item Decoraion;
    public Item StartStone;
    public Item EnhanceStone;
    // 初始化所有地块
    public void ResetAllTiles()
    {
        tiles = new EnhanceTile[TILE_COUNT];
        for (int i = 0; i < TILE_COUNT; i++)
        {
            tiles[i] = new EnhanceTile
            {
                id = -1,         // 0表示空地
                Num = 0,
                EnhanceItem = null,
            };
            Decoraion = null;
            StartStone = null;
            EnhanceStone = null;
        }
    }
    // 初始化一个地块
    public void ResetATiles(int tileIndex)
    {
        tiles[tileIndex] = new EnhanceTile
        {
            id = -1,         // 0表示空地
            Num = 0,
            EnhanceItem = null,
        };
    }
    public int AddtionEnhanction(int id0,Item item)
    {
        int index = -1;
        bool IsAdd = true;
        for (int i = 0; i < TILE_COUNT; i++)
        {
            if (tiles[i].id != -1)
            {
                if (tiles[i].id == id0)
                {
                    if (item.IsStack)
                    {
                        tiles[i].Num++;
                        IsAdd = false;
                        index = i;
                        break;
                    }
                }

            }
        }
        if(IsAdd)
        {
            for (int i = 0; i < TILE_COUNT; i++)
            {

                if (tiles[i].id == -1)
                {
                    tiles[i] = new EnhanceTile
                    {
                        id = item.ItemId,         // 0表示空地
                        Num = 1,
                        EnhanceItem = item,

                    };
                    index = i;
                    break;
                }

            }
        }

        return index;


    }
    public bool CanEnhanceWithRecipeStrict(Recipe recipe,int index  )
    {
        // 先统计 tiles 中所有物品的数量（优化性能）
        Dictionary<int, int> itemCounts = tiles
            .Where(tile => tile.EnhanceItem != null)
            .GroupBy(tile => tile.EnhanceItem.ItemId)
            .ToDictionary(g => g.Key, g => g.Sum(tile => tile.Num));
        var ingredient = recipe.ingredients[index];
        
        for (int i = 0; i < ingredient.item.Count; i++)
        {
            int itemId = ingredient.item[i].ItemId;
            int requiredAmount = ingredient.amount[i];

            if (!itemCounts.TryGetValue(itemId, out int availableAmount) ||
                availableAmount < requiredAmount)
            {
                return false; // 数量不足
            }
        }

        return true;
    }

}
[System.Serializable]
public struct EnhanceTile
{
    public int id;
    public int Num;
    public Item EnhanceItem;
}