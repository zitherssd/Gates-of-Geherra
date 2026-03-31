using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor;

public enum ItemTrigger
{
    OnNewFloor,
    OnNewBattle,
    OnHpBarBroken,
    OnDamageDealt,
    OnDamageTaken,
}

public class ItemEventArgs
{
    public Actor source;
    public object extra;
}