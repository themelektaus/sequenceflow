namespace Prototype.SequenceFlow
{
    public class EventArgs
    {
        public readonly string eventType;

        public EventArgs(string eventType = "")
        {
            this.eventType = eventType;
        }
    }
}
