using Avak.StateMachine.Core.Contracts;
using System.Diagnostics;

namespace Avak.StateMachine.Core.States
{
    [DebuggerDisplay("Name: {Name}")]
    public abstract class StateBase : IEquatable<StateBase>
    {
        public bool IsInitial { get; set; } = false;

        public string Id => this.GetType().FullName!;

        public string Name => this.GetType().Name;

        public List<Transition> Transitions { get; set; }

        protected StateBase()
        {
            Transitions = [];
        }

        public override string ToString()
        {
            // This needs to be improved.
            return Id;
        }

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

        public override bool Equals(object? obj)
        {
            return Equals(obj as StateBase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }

        protected virtual void Init()
        {

        }

        internal void InternalInit()
        {
            Init();
        }

        protected virtual void Enter()
        {

        }

        internal void InternalEnter()
        {
            Enter();
        }

        protected virtual void Exit()
        {

        }

        internal void InternalExit()
        {
            Exit();
        }

        public abstract IStateViewModel GetStateViewModel();
    }
}
