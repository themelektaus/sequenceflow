using System;
using System.Linq;

namespace Prototype.SequenceFlow
{
    [Serializable]
    public class StatementMethod : Method
    {
        [NonSerialized] StatementMethodDefinition definition = null;
        [NonSerialized] bool definitionInitialized = false;

        public bool Invoke(Statement statement, SimpleData parameters)
        {
            if (!definitionInitialized)
            {
                definitionInitialized = true;
                definition = StatementMethodDefinition.CreateInstance(name);
            }

            if (definition is null)
            {
                var method = statement.GetType().GetMethod(name);
                if (method is not null)
                {
                    var types = method.GetParameters().Select(x => x.ParameterType);
                    return (bool) method.Invoke(statement, GetParameters(types, parameters));
                }
            }
            else
            {
                var types = definition.fieldInfos.Select(x => x.FieldType);
                return definition.Invoke(statement, GetParameters(types, parameters));
            }

            return false;
        }
    }
}
