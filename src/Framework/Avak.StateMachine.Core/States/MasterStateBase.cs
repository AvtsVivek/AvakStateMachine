using Avak.StateMachine.Core.Contracts;

namespace Avak.StateMachine.Core.States
{
    public class MasterStateBase : SingleStateBase
    {
        public MasterStateBase()
        {
            HyrarchyLevel = 0;
        }
        public override IStateViewModel GetStateViewModel()
        {
            throw new NotImplementedException();
        }
    }
}
