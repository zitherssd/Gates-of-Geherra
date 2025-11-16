using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Assets.Scripts.Battle.Actor.AI.Behaviors
{

    public enum NodeState { Sucess, Failure, Running }
    public abstract class BTNode
    {
        public abstract NodeState Execute(AIBT ai, Actor actor);
    }

    public class SelectorNode : BTNode // Selectors are used to choose the first successful child node
    {
        private readonly List<BTNode> children;

        public SelectorNode(List<BTNode> children)
        {
            this.children = children;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            foreach (var child in children)
            {
                var state = child.Execute(ai, actor);
                if (state == NodeState.Sucess || state == NodeState.Running)
                {
                    return state;
                }
            }
            return NodeState.Failure; //All children failed, return failure state
        }
    }

    public class SequenceNode : BTNode // Sequences are used to execute child nodes in order until one fails
    {
        private readonly List<BTNode> children;

        public SequenceNode(List<BTNode> children)
        {
            this.children = children;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            foreach (var child in children)
            {
                var state = child.Execute(ai, actor);
                if (state != NodeState.Sucess)
                {
                    return NodeState.Failure; // Fail or Running
                }
            }
            return NodeState.Sucess; // All children succeeded, return success state
        }
    }

    public class ConditionNode : BTNode
    {
        private readonly System.Func<Actor, bool> condition;

        public ConditionNode(System.Func<Actor, bool> condition)
        {
            this.condition = condition;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            Debug.WriteLine("Evaluating condition:" + condition.ToString());
            if (condition(actor))
            {
                return NodeState.Sucess;
            }
            return NodeState.Failure;
        }
    }

    public class ActionNode : BTNode
    {
        private readonly System.Func<Actor> condition;

        public ActionNode(System.Func<Actor> condition)
        {
            this.condition = condition;
        }

        public override NodeState Execute(AIBT ai, Actor actor)
        {
            condition?.Invoke();
                return NodeState.Sucess;
        }
    }
}

