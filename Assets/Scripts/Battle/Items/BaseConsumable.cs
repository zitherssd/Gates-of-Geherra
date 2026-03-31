using Assets.Scripts.Battle.Actions.Actions.Effects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Items
{
    [CreateAssetMenu(fileName = "Consumable", menuName = "ScriptableObjects/Item/Consumable")]
    public class BaseConsumable : BaseItem
    {


        [SerializeReference, SubclassSelector]
        public List<IItemEffect> Effects = new List<IItemEffect>();

        //public void UseConsumable(BaseConsumable item)
        //{
        //    foreach (var effect in item.Effects)
        //        effect.Eval(actor, null); // no trigger args needed

        //    if (item.stackable)
        //        RemoveOne(item);
        //    else
        //        Remove(item);
        //}
    }
}
