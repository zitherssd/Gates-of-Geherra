using Assets.Scripts.Battle.Manager;
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
            // Resolve the current player body (battle/arena via BattleManager, else the rest scene's
            // GameFlowManager) so the inventory works in any scene.
            var player = BattleManager.instance != null ? BattleManager.instance.Player : null;
            if (player == null && GameFlowManager.instance != null)
                player = GameFlowManager.instance.playerActor;
            if (player == null) return;

            inventory = player.inventory;
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
