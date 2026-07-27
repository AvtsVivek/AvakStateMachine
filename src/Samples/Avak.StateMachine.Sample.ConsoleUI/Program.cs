using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Sample.ConsoleUI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            PopulateStateListUsingFileStream();

        }

        private static void PopulateStateListUsingFileStream()
        {
            string appStateTable = "Avak.StateMachine.Sample.ConsoleUI.StateManager.State.xml";


            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), appStateTable);

            stateMachineManager.LoadMasterStateFile();

            IStateGraph stateGraph = stateMachineManager.GetCurrentStateGraph();

            List<Trigger> triggers = stateMachineManager.StateGraph.TriggerList;

            Console.WriteLine($"The number of Triggers in the state file are {triggers.Count}");

            Console.WriteLine($"The number of states in the state file are {stateGraph.StateList.Count}");
        }
    }
}
