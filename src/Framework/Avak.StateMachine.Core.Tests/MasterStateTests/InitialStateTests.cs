using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.MasterStateTests
{
    /// <summary>
    /// If no Initial attribute is specified for a state element, then the very first one will be treated as initial state.
    /// If initial state is specified for a state, then that state will be taken as the initial.
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
            StateXmlFileTree.Instance.Clear();
        }

        [TestMethod]
        public void NoInitialSpecified_TopStateIsDefaultInitial()
        {
            // Arrange
            string masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.NoInitialStateSpecified.xml";
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);


            // Act 
            IStateGraph stateGraph = stateMachineManager.GetCurrentStateGraph();
            StateBase stateBb = stateGraph.StateList.FirstOrDefault(state => state.Name == "Bb")!;

            // Assert
            Assert.IsNotNull(stateBb);
            Assert.IsTrue(stateBb.IsInitial);
            Assert.AreEqual(stateBb, stateMachineManager.CurrentState);
        }

        [TestMethod]
        public void InitialSpecified_TopStateIsInitial()
        {
            // Arrange

            string masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.InitialStateSpecified.xml";
            IXmlKeys constants = new XmlKeys();
            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);
            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);


            // Act 
            IStateGraph stateGraph = stateMachineManager.GetCurrentStateGraph();

            StateBase stateBb = stateGraph.StateList.FirstOrDefault(state => state.Name == "Bb")!;

            // Assert
            Assert.IsNotNull(stateBb);
            Assert.IsTrue(stateBb.IsInitial);
            Assert.AreEqual(stateBb, stateMachineManager.CurrentState);
        }
    }
}
