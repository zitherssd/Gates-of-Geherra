using Assets.Scripts.Battle.Actions.Actions.Effects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Items
{
    [CreateAssetMenu(fileName = "Trinket", menuName = "ScriptableObjects/Items/Trinket")]
    public class BaseTrinket : BaseItem
    {

        public List<ItemTrigger> Triggers = new List<ItemTrigger>();

        [SerializeReference, SubclassSelector]
        public List<IItemEffect> Effects = new List<IItemEffect>();

        /// <summary>
        /// Call this when the item is added to an actor's inventory.
        /// </summary>
        public void Equip(Actor.Actor owner)
        {
            foreach (var trigger in Triggers)
                foreach (var effect in Effects)
                    ItemEventBus.Subscribe(trigger, owner, effect);
        }

        /// <summary>
        /// Call this when removing from inventory.
        /// </summary>
        public void Unequip(Actor.Actor owner)
        {
            foreach (var trigger in Triggers)
                foreach (var effect in Effects)
                    ItemEventBus.Unsubscribe(trigger, owner, effect);
        }
    }

}
