using Avak.StateMachine.Core.Contracts;

namespace Avak.StateMachine.Core.States
{
    public class MasterStateBase : SingleStateBase
    {
        public override IStateViewModel GetStateViewModel()
        {
            throw new NotImplementedException();
        }
    }
}
