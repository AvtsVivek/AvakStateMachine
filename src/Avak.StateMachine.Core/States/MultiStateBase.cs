using Avak.StateMachine.Core.Contracts;

namespace Avak.StateMachine.Core.States
{
    /// <summary>
    /// Base class which represents multiple number of states the app is simultaneously present in.
    /// Sometimes this is called as ParallelState.
    /// </summary>
    public class MultiStateBase : StateBase
    {
        public override IStateViewModel GetStateViewModel()
        {
            throw new NotImplementedException();
        }
    }
}
