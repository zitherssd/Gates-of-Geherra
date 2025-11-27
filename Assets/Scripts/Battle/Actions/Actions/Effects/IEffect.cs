using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Actor.AI.Behaviors;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    public interface IEffect
    {
        void Eval(Actor.Actor actor, BaseAction action);
    }
    
    public interface IProjectileEffect
    {
        void Eval(Actor.Actor actor, ProjectileAttack action, GameObject projectile);
    }

    public interface IEndableEffect
    {
        void End(Actor.Actor actor, BaseAction action);
    }

    public interface IUpdateableEffect
    {
        void Update(Actor.Actor actor, BaseAction action, float deltaTime);
    }

    public interface IHitbox
    {
        public List<Actor.Actor> CheckEnemiesInsideHitbox(Actor.Actor casterActor);
    }

    public interface IItemEffect
    {
        void Eval(Actor.Actor owner);
    }

}

