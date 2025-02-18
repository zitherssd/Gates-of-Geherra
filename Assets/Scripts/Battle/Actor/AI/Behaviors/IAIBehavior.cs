using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Battle.Components.AI.Behaviors
{
    public interface IAIBehavior
    {
        bool Execute(AISystem ai, Actor actor);
    }
}
