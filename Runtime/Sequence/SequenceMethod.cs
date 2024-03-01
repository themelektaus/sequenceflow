using System;
using System.Collections;
using System.Linq;

namespace Prototype.SequenceFlow
{
    [Serializable]
    public class SequenceMethod : Method
    {
        [NonSerialized] SequenceMethodDefinition definition = null;
        [NonSerialized] bool definitionInitialized = false;

        public SequenceMethod() : base()
        {

        }

        public SequenceMethod(string name, params object[] parameters) : base(name, parameters)
        {

        }

        public IEnumerator Invoke(Sequence sequence, SimpleData parameters)
        {
            if (!definitionInitialized)
            {
                definitionInitialized = true;
                definition = SequenceMethodDefinition.CreateInstance(name);
            }

            if (definition is not null)
            {
                if (definition is AsyncSMD asyncDefinition)
                {
                    var types = definition.fieldInfos.Select(x => x.FieldType);
                    return asyncDefinition.Invoke(sequence, GetParameters(types, parameters));
                }

                if (definition is SMD baseDefinition)
                {
                    var types = definition.fieldInfos.Select(x => x.FieldType);
                    baseDefinition.Invoke(sequence, GetParameters(types, parameters));
                }
            }

            return null;
        }

        public void OnExitSequence()
        {
            definition?.OnExitSequence();
        }
    }
}
