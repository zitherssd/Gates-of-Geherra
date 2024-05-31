namespace Assets.Scripts.Battle.Components.Status

{
    public class Poison : BaseStatus
    {

        //expires dupa 5 runde

        public Poison(Actor owner)
        {

        }
        public override void Apply()
        {
            owner.DamageRecieved += ModifyDamage;
        }

        public override void Tick()
        {
            //verific daca au trecut 5 runde

            //
            owner.statusManager.Remove(this);
        }

        public float ModifyDamage(float damage)
        {
            float modifiedDamage = damage * 0.5f;
            return modifiedDamage;
        }

    }
}