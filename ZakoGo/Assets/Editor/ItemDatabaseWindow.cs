using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemDatabaseWindow : EditorWindow
{
    private Vector2 scrollPos;
    private string searchFilter = "";
    private ItemType typeFilter = ItemType.物品;

    [MenuItem("Tools/Item Database")]
    public static void ShowWindow()
    {
        GetWindow<ItemDatabaseWindow>("Item Database");
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        searchFilter = EditorGUILayout.TextField("搜索", searchFilter);
        typeFilter = (ItemType)EditorGUILayout.EnumPopup("类型过滤", typeFilter);
        GUILayout.EndHorizontal();

        List<Item> allItems = FindAllItems();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var item in allItems)
        {
            if (!string.IsNullOrEmpty(searchFilter) &&
                !item.ItemName.ToLower().Contains(searchFilter.ToLower()) &&
                !item.ItemId.ToString().Contains(searchFilter))
            {
                continue;
            }

            if (typeFilter != ItemType.物品 && item.itemtype != typeFilter)
            {
                continue;
            }

            GUILayout.BeginHorizontal("box");

            // 显示物品图片
            if (item.ItemImg != null)
            {
                EditorGUILayout.ObjectField(item.ItemImg, typeof(Sprite), false, GUILayout.Width(50), GUILayout.Height(50));
            }
            else
            {
                GUILayout.Label("无图片", GUILayout.Width(50), GUILayout.Height(50));
            }

            GUILayout.BeginVertical();

            // 显示物品基本信息
            GUILayout.Label($"{item.ItemName} (ID: {item.ItemId})", EditorStyles.boldLabel);
            GUILayout.Label($"类型: {item.itemtype} | 品质: {item.ItemValue}");

            // 显示数量信息
            GUILayout.Label($"数量 - 仓库: {item.BoxItemNum} | 背包: {item.BagItemNum} | 其他: {item.OtherItemNum}");

            GUILayout.EndVertical();

            // 添加编辑按钮
            if (GUILayout.Button("编辑", GUILayout.Width(60), GUILayout.Height(50)))
            {
                Selection.activeObject = item;
                EditorGUIUtility.PingObject(item);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        EditorGUILayout.EndScrollView();

        // 添加创建新物品的按钮
        if (GUILayout.Button("创建新物品"))
        {
            CreateNewItem();
        }
    }

    private List<Item> FindAllItems()
    {
        string[] guids = AssetDatabase.FindAssets("t:Item");
        List<Item> items = new List<Item>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Item item = AssetDatabase.LoadAssetAtPath<Item>(path);
            items.Add(item);
        }

        return items.OrderBy(i => i.ItemId).ToList();
    }

    private void CreateNewItem()
    {
        Item newItem = CreateInstance<Item>();

        // 设置默认值
        newItem.ItemName = "新物品";
        newItem.itemtype = ItemType.物品;
        newItem.ItemValue = ItemValueType.普通;

        string path = EditorUtility.SaveFilePanelInProject(
            "保存新物品",
            "NewItem.asset",
            "asset",
            "请输入保存新物品的文件名");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newItem, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = newItem;
        }
    }
}