using Assets.Scripts.Battle.Actions.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Actions.Effects
{
    [Serializable]
    public class PlayParticleEffect : IEffect, IEndableEffect
    {
        public ParticleSystem psPrefab;
        private ParticleSystem instance;
        public bool OnPlayer = true;

        public void End(Actor.Actor actor, BaseAction action)
        {
           if(instance != null)
           {
                instance.Stop();
           }
        }

        public void Eval(Actor.Actor actor, BaseAction action)
        {
            if (psPrefab == null) return;
            if(instance == null)
            {
                if (OnPlayer)
                {
                    instance = GameObject.Instantiate(psPrefab, actor.transform);
                }
                else
                {
                    var gs = action as GenericSkill;
                    var HitboxEffect = gs.OnStartEffects.Where(item => item is ShowHitboxOnPoint).FirstOrDefault() as ShowHitboxOnPoint;
                    instance = GameObject.Instantiate(psPrefab, HitboxEffect.point, actor.transform.rotation);
                }
            }
            if(!OnPlayer)
            {
                var gs = action as GenericSkill;
                var HitboxEffect = gs.OnStartEffects.Where(item => item is ShowHitboxOnPoint).FirstOrDefault() as ShowHitboxOnPoint;
                instance.transform.position = HitboxEffect.point;
            }
            instance.Play();
        }
    }
}
