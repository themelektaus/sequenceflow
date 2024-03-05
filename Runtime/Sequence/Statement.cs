using System;
using System.Linq;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    [Serializable]
    public partial class Statement
    {
        public LogicGate logicGate;
        public StatementCondition[] conditions;


        public Transform activator { get; private set; }
        public MonoBehaviour owner { get; private set; }
        public Transform executer { get; private set; }
        public EventArgs e { get; private set; }
        public SimpleData parameters { get; private set; }

        public Statement()
        {

        }

        public bool Check(Transform activator, MonoBehaviour owner, EventArgs e, SimpleData parameters)
        {
            this.activator = activator;
            this.owner = owner;
            this.e = e;
            this.parameters = parameters;

            if (conditions is null)
                return true;

            var enabledConditions = conditions.Where(x => x.enabled).ToArray();
            if (enabledConditions.Length == 0)
                return true;

            int trueCounter = 0;

            foreach (var condition in enabledConditions)
            {
                executer = condition.executer == Command.Executer.Activator
                    ? activator
                    : owner.transform;

                var conditionResult = condition.method.Invoke(this, parameters);
                if (conditionResult)
                    trueCounter++;

                switch (logicGate)
                {
                    case LogicGate.And:
                        if (!conditionResult)
                            return false;
                        break;

                    case LogicGate.Or:
                        if (conditionResult)
                            return true;
                        break;

                    case LogicGate.Xor:
                        if (conditionResult && trueCounter > 1)
                            return false;
                        break;
                }
            }

            return logicGate switch
            {
                LogicGate.And => true,
                LogicGate.Or => false,
                LogicGate.Xor => trueCounter == 1,
                _ => false,
            };
        }
    }
}
