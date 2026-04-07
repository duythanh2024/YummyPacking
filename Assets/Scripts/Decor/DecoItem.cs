using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DecoOption
{
    public int optionId;
    public string optionName;
    public Sprite icon;       // Icon hiển thị trong Shop
    public bool active;   // Giá nếu muốn bán thêm option
}
[CreateAssetMenu(fileName = "NewDecoItem", menuName = "Restaurant/Deco Item")]
public class DecoItem : ScriptableObject
{
    public int id;
    public string content;
    public int cost; // Số sao ban đầu
    public List<int> prerequisiteIds;
    public bool isFinish;
    
    [Header("Options")]
    public List<DecoOption> options; // Danh sách các mẫu mã (Model 1, 2, 3)
}
