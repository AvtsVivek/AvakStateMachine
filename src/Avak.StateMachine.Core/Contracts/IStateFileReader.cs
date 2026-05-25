namespace Avak.StateMachine.Core.Contracts
{
	internal interface IStateFileReader
	{
		void SetStateFile(Stream stream);

		void SetStateFilePath(string filePath);

		bool LoadStateFile();

		string GetRootNamespace();

		List<Trigger> GetTriggers();

		StateGraph GetStateGraph(StateDependencyObjectFinder stateDependencyObjectFinderDelegate);
	}
}
