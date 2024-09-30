using Assets.Scripts.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    public class ActionsRepository
    {


        static Teleport NinjutsuTP = Resources.Load<BaseAction>("Skills/Teleport") as Teleport;
        static Teleport ShadowStep = Resources.Load<BaseAction>("Skills/Shadowstep") as Teleport;
    }
}