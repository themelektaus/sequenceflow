using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    [CreateAssetMenu]
    public class SequenceFlowObject : ScriptableObject
    {
        public bool embeddedInScene;
        public GameObject embeddedInPrefab;
        [TextArea(8, 8)] public string data;

        public SequenceFlow sequenceFlow = new();
        public List<Sequence> stateSequences = new();
        public List<Statement> transitionStatements = new();

        public SequenceFlowObject()
        {
            sequenceFlow.sequenceFlowObject = this;
        }

        public void ReadFromData()
        {
            if (!string.IsNullOrEmpty(data))
            {
                var data = this.data;

                sequenceFlow = JsonUtility.FromJson<SequenceFlow>(data);
                sequenceFlow.startState = sequenceFlow.states.Where(x => x.guid == sequenceFlow.startStateGUID).FirstOrDefault();

                foreach (var state in sequenceFlow.states)
                    state.sequenceFlow = sequenceFlow;

                foreach (var transition in sequenceFlow.transitions)
                {
                    transition.sequenceFlow = sequenceFlow;
                    transition.source = sequenceFlow.states.Where(x => x.guid == transition.sourceGuid).FirstOrDefault();
                    transition.destination = sequenceFlow.states.Where(x => x.guid == transition.destinationGuid).FirstOrDefault();
                }
            }

            sequenceFlow.sequenceFlowObject = this;
        }

        public void AbortFlow()
        {
            sequenceFlow.Abort();
        }

#if UNITY_EDITOR
        public bool WriteToData(bool saveAssets = false)
        {
            if (sequenceFlow.states.Any(x => x.sequenceFlow is null))
                return false;

            var newData = GetSerializedData();

            if (data == newData)
                return false;

            data = newData;

            UnityEditor.EditorUtility.SetDirty(this);

            if (saveAssets)
                UnityEditor.AssetDatabase.SaveAssets();

            return true;
        }

        string GetSerializedData()
        {
            if (sequenceFlow.startState is not null)
                sequenceFlow.startStateGUID = sequenceFlow.startState.guid;

            foreach (var transition in sequenceFlow.transitions)
            {
                transition.sourceGuid = transition.source.guid;
                transition.destinationGuid = transition.destination.guid;
            }

            return JsonUtility.ToJson(sequenceFlow);
        }
#endif
    }
}
