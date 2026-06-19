using Assets.Scripts.Battle.Actions.Actions.Effects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.HitWindows
{
    [Serializable]
    public struct HitWindow
    {
        public string displayName;                              // "Jab", "Dash Active", etc.
        public int startFrame;                                 // Inclusive start frame
        public int endFrame;                                   // Inclusive end frame
        
        public int maxHitsPerEnemy;                            // 0 = unlimited; 1 = once; 2 = twice; etc.
        public int maxTriggersPerPlayer;                       // 0 = unlimited; limits how often this window's effects trigger for caster
        
        [SerializeReference, SubclassSelector]
        public List<IEffect> windowEffects;                   // Effects to run while active
    }
    
    [System.Serializable]
    public struct AnimationPhase
    {
        public ANIMATION animation;                            // Animation state to play
        public float windupTimeMult;                            // Animator speed during windup phase
        public float recoveryTimeMult;                          // Animator speed during recovery phase
        public List<HitWindow> hitWindows;                     // Hit windows for this animation
    }
}

