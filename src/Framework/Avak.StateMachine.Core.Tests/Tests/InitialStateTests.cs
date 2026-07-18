using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.Tests
{
    /// <summary>
    /// If no Initial State is specified, then the very first one will be treated as initial state.
    /// If initial state is specified, then that state will be taken as the initial.
    /// </summary>
    [TestClass]
    public class InitialStateTests
    {
        [TestInitialize]
        public void Setup()
        {

        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void NoInitialSpecified_TopStateIsDefaultInitial()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.NoInitialStateSpecified.xml";
            Stream resourceStream = assembly.GetManifestResourceStream(appStateFile)!;
            IXmlKeys constants = new XmlKeys();
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetStateFile(resourceStream);
            bool loadResult = stateMachineManager.LoadStateFile();

            // Act 
            IStateGraph stateGraph = stateMachineManager.GetStateGraph();
            StateBase stateBb = stateGraph.StateList.FirstOrDefault(state => state.Name == "Bb")!;

            // Assert
            Assert.IsNotNull(stateBb);
            Assert.IsTrue(loadResult);
            Assert.IsTrue(stateBb.IsInitial);
            Assert.AreEqual(stateBb, stateMachineManager.CurrentState);
        }

        [TestMethod]
        public void InitialSpecified_TopStateIsInitial()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();
            string appStateFile = "Avak.StateMachine.Core.Tests.StateManager.InitialStateSpecified.xml";
            Stream resourceStream = assembly.GetManifestResourceStream(appStateFile)!;
            IXmlKeys constants = new XmlKeys();
            StateMachineManager stateMachineManager = new(constants, StateDependencyImplimentation.StateDependencyObjectFinderDefaultImplimentation);
            stateMachineManager.SetStateFile(resourceStream);
            bool loadResult = stateMachineManager.LoadStateFile();

            // Act 
            IStateGraph stateGraph = stateMachineManager.GetStateGraph();
            // stateMachineManager.Initialize();
            StateBase stateBb = stateGraph.StateList.FirstOrDefault(state => state.Name == "Bb")!;

            // Assert
            Assert.IsNotNull(stateBb);
            Assert.IsTrue(loadResult);
            Assert.IsTrue(stateBb.IsInitial);
            Assert.AreEqual(stateBb, stateMachineManager.CurrentState);
        }
    }
}
