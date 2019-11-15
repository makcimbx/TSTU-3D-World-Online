using UnityEngine;
namespace TSTU.Controller
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class Item : ScriptableObject
    {
        new public string name = "simple text";
        public Sprite icon = null;
        public bool isDefaultItem = false;
        public int price = 0;
        public GameObject prefab = null;
    }

}