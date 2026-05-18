namespace Avak.StateMachine.Core
{
    public abstract class StateBase : IEquatable<StateBase>
    {
        public bool IsInitial { get; set; } = false;

        protected StateBase()
        {
            name = string.Empty;
            Transitions = [];
        }

        public string Id
        {
            get => this.GetType().Name;
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<Transition> Transitions { get; set; }

        public bool Equals(StateBase? other)
        {
            if (other == null)
            {
                return false;
            }

            if (!ReferenceEquals(this, other))
            {
                return false;
            }

            // Compare properties for equality
            return this.Id == other.Id
                && this.Name == other.Name;
        }
    }
}
