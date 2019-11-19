namespace TSTU.Controller
{
    public class ItemPickup : Interacteble
    {
        public Item item;
      
        public override void Interact(PlayerController player)
        {
            base.Interact(player);
            if (isInteracteble)
            {
                if (!Inventory.instance.isFull)
                {
                    Inventory.instance.Add(item);
                    Destroy(gameObject);
                }
              
            }

        }
    }
}