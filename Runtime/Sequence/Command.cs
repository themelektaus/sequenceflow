
namespace Prototype.SequenceFlow
{
    public abstract class Command
    {
        public enum Executer { Owner, Activator }

        public bool enabled;
        public Executer executer;

        public Command()
        {
            enabled = true;
            executer = Executer.Owner;
        }
    }
}
