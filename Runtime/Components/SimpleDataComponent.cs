using UnityEngine;

namespace Prototype.SequenceFlow
{
    [AddComponentMenu("/")]
    [DisallowMultipleComponent]
    public class SimpleDataComponent : MonoBehaviour
    {
        public SimpleData simpleData = new();
    }
}
