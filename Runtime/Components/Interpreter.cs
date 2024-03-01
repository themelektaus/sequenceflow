using UnityEngine;

namespace Prototype.SequenceFlow
{
    public abstract class Interpreter : MonoBehaviour, IReceiver
    {
        void Start()
        {
            OnStart();
        }

        public bool OnReceive(Transform sender, Transform reveiver, EventArgs e)
        {
            if (!enabled)
                return false;

            OnReceive(sender, e);
            return true;
        }

        public void Abort()
        {
            OnAbort();
        }

        protected abstract void OnStart();
        protected abstract void OnReceive(Transform sender, EventArgs e);
        protected abstract void OnAbort();

        public abstract void Perform(Transform activator, EventArgs e);
    }
}
