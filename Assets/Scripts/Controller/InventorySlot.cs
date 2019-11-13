using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Button button;
    public ItemButton itemButton;
    private Item item;

    public void AddItem(Item item)
    {
        this.item = item;
        icon.sprite = item.icon;
        icon.enabled = true;
        button.interactable = true;
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
        button.interactable = false;
    }

    private void OnDrag()
    {
        Debug.Log(123 + "213");
    }
}
