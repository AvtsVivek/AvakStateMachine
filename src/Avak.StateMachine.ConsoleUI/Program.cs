using Avak.StateMachine.Core;
using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.ConsoleUI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // PopulateStateListUsingFileStream();

            PopulateStateListUsingFilePath();
        }

        private static void PopulateStateListUsingFilePath()
        {
            string filePath = $"C:\\Work\\sbc_sw\\sbc\\app\\core\\ExecUI\\Infrastructure\\StateManager\\AppStateTable1.xml";

            string outputFolderPath = $"C:\\Work\\sbc_sw\\sbc\\app\\core\\ExecUI\\UIShell\\bin\\x64\\Debug\\net10.0-windows\\win-x64";

            if (!File.Exists(filePath))
            {
                return;
            }

            IXmlKeys constants = new ThorKeys();

            // IStateFileReader stateFileReader = new XmlStateFileReader(constants);

            StateMachineManager stateMachineManager = new StateMachineManager(constants);

            stateMachineManager.SetStateFilePath(filePath);

            bool isLoaded = stateMachineManager.LoadStateFile();

            StateGraph stateGraph = stateMachineManager.GetStateGraph();

            List<Trigger> triggers = stateGraph.TriggerList;

            Console.WriteLine($"The number of Triggers in the state file are {triggers.Count}");

            //Console.WriteLine($"The number of states in the state file are {states.Count}");

        }

        private static void PopulateStateListUsingFileStream()
        {
            var assembly = Assembly.GetExecutingAssembly();

            string appStateTable = "Avak.StateMachine.ConsoleUI.StateManager.State.xml";

            using Stream stream = assembly.GetManifestResourceStream(appStateTable)!;

            if (stream == null)
            {
                return;
            }

            IXmlKeys constants = new XmlKeys();

            // IStateFileReader stateFileReader = new XmlStateFileReader(constants);

            StateMachineManager stateMachineManager = new StateMachineManager(constants);

            stateMachineManager.SetStateFile(stream);

            bool isLoaded = stateMachineManager.LoadStateFile();

            StateGraph stateGraph = stateMachineManager.GetStateGraph();

            List<Trigger> triggers = stateMachineManager.StateGraph.TriggerList;

            Console.WriteLine($"The number of Triggers in the state file are {triggers.Count}");

            Console.WriteLine($"The number of states in the state file are {stateGraph.StateList.Count}");
        }
    }
}
