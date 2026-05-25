namespace Avak.StateMachine.Core.Contracts
{
	public interface ITypeFinder
	{
		bool TryFindType(string typeFullName, out Type type);
	}
}
