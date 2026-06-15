using UnityEngine;
// Базовый класс Item
public class BaseItem  : ScriptableObject
{
    public FieldLocalize itemName;
    public Sprite itemIcon;
    public GameObject itemPrefab;
    public GameObject itemModel;
}

// Класс для ключей
[CreateAssetMenu(fileName = "Key", menuName = "Super3DGame/Items/Key")]
public class KeyItem : BaseItem 
{
    public int keyLevel = 1;
    public string keyID = "";
    
}

// Класс для лечения
[CreateAssetMenu(fileName = "HealItem", menuName = "Super3DGame/Items/Heal")]
public class HealItem : BaseItem 
{
    public int healAmount = 20;
}

// Класс для автомобильных предметов
[CreateAssetMenu(fileName = "CarAttribute", menuName = "Super3DGame/Items/CarAttribute")]
public class CarAttributeItem : BaseItem 
{
    
}
