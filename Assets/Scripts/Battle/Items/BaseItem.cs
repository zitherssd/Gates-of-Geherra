using Assets.Scripts.Battle.Actions.Actions.Effects;
using Assets.Scripts.Battle.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item")]
    public class BaseItem : ScriptableObject
    {
        [Header("Item Info")]
        public string ItemName;
        public Sprite Icon;

        public List<ItemTrigger> Triggers = new List<ItemTrigger>();

        [SerializeReference, SubclassSelector]
        public List<IItemEffect> Effects = new List<IItemEffect>();

        /// <summary>
        /// Call this when the item is added to an actor's inventory.
        /// </summary>
        public void SubscribeToActor(Actor.Actor owner)
        {
            foreach (var trigger in Triggers)
                foreach (var effect in Effects)
                    ItemEventBus.Subscribe(trigger, owner, effect);
        }

        /// <summary>
        /// Call this when removing from inventory.
        /// </summary>
        public void UnsubscribeFromActor(Actor.Actor owner)
        {
            foreach (var trigger in Triggers)
                foreach (var effect in Effects)
                    ItemEventBus.Unsubscribe(trigger, owner, effect);
        }
    }


}
