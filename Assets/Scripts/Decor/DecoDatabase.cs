using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "DecoDatabase", menuName = "Restaurant/Deco Database")]
public class DecoDatabase : ScriptableObject
{
    public System.Collections.Generic.List<DecoItem> allItems;

    public DecoItem GetItemById(int id)
    {
        return allItems.Find(item => item.id == id);
    }
}