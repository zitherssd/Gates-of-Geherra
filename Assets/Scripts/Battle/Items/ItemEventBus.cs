using System;
using System.Collections.Generic;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Actions.Effects;

public static class ItemEventBus
{
    private static readonly Dictionary<ItemTrigger,
        List<(Actor owner, IItemEffect effect)>> listeners
        = new();

    public static void Subscribe(ItemTrigger trigger, Actor owner, IItemEffect effect)
    {
        if (!listeners.ContainsKey(trigger))
            listeners[trigger] = new();

        listeners[trigger].Add((owner, effect));
    }

    public static void Unsubscribe(ItemTrigger trigger, Actor owner, IItemEffect effect)
    {
        if (!listeners.ContainsKey(trigger)) return;

        listeners[trigger].Remove((owner, effect));
    }

    public static void Raise(ItemTrigger trigger)
    {
        if (!listeners.ContainsKey(trigger)) return;

        foreach (var (owner, effect) in listeners[trigger])
            effect.Eval(owner);   // <-- Run effect on the owner
    }

    public static void Raise(ItemTrigger trigger, Actor specificOwner)
    {
        if (!listeners.ContainsKey(trigger)) return;

        foreach (var (owner, effect) in listeners[trigger])
            if (owner == specificOwner)   // <--- only fire for that actor
                effect.Eval(owner);
    }
}
