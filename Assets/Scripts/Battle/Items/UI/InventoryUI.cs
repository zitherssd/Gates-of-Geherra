using Assets.Scripts.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.Items.UI
{
    public class InventoryUI : MonoBehaviour
    {
        public ActorInventory inventory;
        public Transform slotParent;
        public GameObject slotPrefab;

        public void Start()
        {
            inventory = GameFlowManager.instance.playerActor.inventory;
            Refresh();
        }

        public void Refresh()
        {
            foreach (Transform child in slotParent)
                Destroy(child.gameObject);

            foreach (var item in inventory.Items)
            {
                var slot = Instantiate(slotPrefab, slotParent);
                slot.GetComponent<ItemSlotUI>().Setup(item, inventory);
            }
        }
    }
}
