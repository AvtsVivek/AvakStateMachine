using Avak.StateMachine.Core.Contracts;

namespace Avak.StateMachine.Core.States
{
    public class SubStateBase : SingleStateBase
    {
        public override IStateViewModel GetStateViewModel()
        {
            throw new NotImplementedException();
        }
    }
}
