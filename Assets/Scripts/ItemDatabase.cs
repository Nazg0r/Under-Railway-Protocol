using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items;

    public Item GetItemById(string id)
    {
        return items.Find(i => i.id == id);
    }
}

[System.Serializable]
public class Item
{
    public string id;
    public string title;
    public Sprite icon;
}
