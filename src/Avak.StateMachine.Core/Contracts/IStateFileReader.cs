namespace Avak.StateMachine.Core.Contracts
{
    public interface IStateFileReader
    {
        void SetStateFile(Stream stream);

        void SetStateFilePath(string filePath);

        bool LoadStateFile();

        string GetRootNamespace();

        List<Trigger> GetTriggers();

        StateGraph GetStateGraph();
    }
}
