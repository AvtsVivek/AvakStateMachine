namespace Avak.StateMachine.Core.Contracts
{
    public interface ITypeFinder
    {
        bool TryFindType(string nameSpace, string typeName, out Type type);
    }
}
