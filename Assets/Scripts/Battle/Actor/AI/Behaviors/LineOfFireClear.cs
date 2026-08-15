namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{
    /// <summary>
    /// Condition node: succeeds when no friendly actor sits in the projectile
    /// corridor between this actor and its target, i.e. a throw would not hit an
    /// ally. Used to gate ranged attacks (e.g. the Shuriken Thrower) so they
    /// avoid friendly fire.
    /// </summary>
    public class LineOfFireClear : BTNode
    {
        private readonly float clearRadius; // corridor half-width that counts as blocked
        private readonly float height;      // height the corridor is checked at

        public LineOfFireClear(float clearRadius = 0.8f, float height = 0.5f)
        {
            this.clearRadius = clearRadius;
            this.height = height;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            return FriendlyFireCheck.IsBlocked(actor, clearRadius, height)
                ? NodeState.Failure
                : NodeState.Sucess;
        }
    }
}
