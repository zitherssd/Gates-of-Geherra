using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Battle.Items.UI
{
    public class ItemSlotUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private BaseItem item;
        private ActorInventory inventory;
        private InventoryUI inventoryUI;
        public Image image;

        private bool isHolding = false;
        private float holdTime = 0.3f; // how long to hold before tooltip appears
        private float holdTimer = 0f;

        private void Awake()
        {
            inventoryUI = GetComponentInParent<InventoryUI>();
        }

        private void Update()
        {
            if (isHolding)
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdTime)
                {
                    // show tooltip
                    string text = $"<color=red>{item.ItemName}</color>\n" + item.Description;
                    TooltipUI.instance.Show(text, Input.mousePosition);
                    isHolding = true; // prevent repeat
                }
            }
        }

        public void Setup(BaseItem item, ActorInventory inventory)
        {
            this.item = item;
            this.inventory = inventory;
            image.sprite = item.Icon;

            // update icon, text, etc.
        }

        public void OnUseClick()
        {
            if(item is BaseConsumable consumable)
            inventory.UseConsumable(consumable);
            inventoryUI.Refresh();

            // Refresh UI so stack count updates
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isHolding = true;
            holdTimer = 0f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // If released before tooltip opens → treat as click
            if (holdTimer < holdTime)
            {
                TryUseItem();
            }

            TooltipUI.instance.Hide();
            isHolding = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // stop holding if pointer leaves
            isHolding = false;
            TooltipUI.instance.Hide();
        }

        private void TryUseItem()
        {
            if (item is BaseConsumable consumable)
                inventory.UseConsumable(consumable);
            inventoryUI.Refresh();
        }
    }
}
