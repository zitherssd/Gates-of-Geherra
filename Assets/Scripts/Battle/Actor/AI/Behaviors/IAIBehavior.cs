namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    public interface IAIBehavior
    {
        bool Execute(AISystem ai, Actor actor);
    }
}
