using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Items;
using System.Collections.Generic;
using UnityEngine;

public class ActorInventory
{
    public Actor Owner { get; private set; }
    public List<BaseItem> Items = new List<BaseItem>();

    public ActorInventory(Actor owner)
    {
        Owner = owner;
    }

    public void AddItem(BaseItem item)
    {
        Items.Add(item);
        item.SubscribeToActor(Owner);
    }

    public void RemoveItem(BaseItem item)
    {
        Items.Remove(item);
        item.UnsubscribeFromActor(Owner);
    }
}
