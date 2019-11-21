using UnityEngine;
namespace TSTU.Controller
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class Item : ScriptableObject
    {
        public int typeId = int.MinValue;
        public long eId = -1;
        public Sprite icon = null;
        public bool isDefaultItem = false;
        public int price = 0;
        public GameObject prefab = null;
    }

}