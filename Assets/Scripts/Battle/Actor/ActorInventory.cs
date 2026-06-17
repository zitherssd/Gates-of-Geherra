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

    /// <summary>
    /// Point this body's inventory at a persistent item list (owned by the run's ActorRuntime)
    /// and wire each trinket's effects to this body. Sharing the list (instead of copying) means
    /// items gained or consumed during play persist automatically when the body is respawned in
    /// another scene.
    /// </summary>
    public void RebindTo(List<BaseItem> sharedItems)
    {
        Items = sharedItems ?? new List<BaseItem>();
        foreach (var item in Items)
        {
            if (item is BaseTrinket trinket)
                trinket.Equip(Owner);
        }
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
