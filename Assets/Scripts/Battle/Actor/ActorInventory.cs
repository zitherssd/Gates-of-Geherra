using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Items;
using Assets.Scripts.Save;
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
        if (item is BaseTrinket trinket)
            trinket.Equip(Owner);
    }

    public void RemoveItem(BaseItem item)
    {
        Items.Remove(item);
        if (item is BaseTrinket trinket)
            trinket.Unequip(Owner);
    }

    public void UseConsumable(BaseConsumable item)
    {
        // Apply effects
        foreach (var effect in item.Effects)
            effect.Eval(Owner);

        // Remove after use
        if (item.stackable)
            RemoveOne(item);
        else
            Remove(item);

        SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
    }

    public void RemoveOne(BaseItem item)
    {
        // For stackable items
        // You probably have a stack count system; placeholder:
        Items.Remove(item);
    }

    public void Remove(BaseItem item)
    {
        Items.Remove(item);
    }
}
