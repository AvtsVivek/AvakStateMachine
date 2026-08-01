using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using Avak.StateMachine.Core.States;
using System.Reflection;
using System.Xml;

namespace Avak.StateMachine.Core.Tests.SubStateTests
{

    [TestClass]
    [DoNotParallelize]
    public class SubStateOneMissingAttributeTests
    {
        private string masterStateXmlFile = string.Empty;
        [TestInitialize]
        public void Setup()
        {

        }

        [TestCleanup]
        public void Cleanup()
        {
            // Runs after each test (clean up files, database connections, etc.)
            // Close the stream.
            StateXmlFileTree.Instance.Clear();
        }

        [TestMethod]
        public void LookForSubStateXmlFileAttribute_NotFoundThrowsException()
        {
            // Arrange
            masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.XmlFilesWithSubStates.MasterStateXmlFileNoSubStateXmlFile.xml";

            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Act
            Exception ex = Assert.Throws<XmlException>(() => stateMachineManager.PopulateStateXmlFileTree());

            string message = "SubStateAssembly attribute is present, but SubStateXmlFile attribute is not present on the state element " + Environment.NewLine +
                $"<State Name=\"Aa\" SubStateAssembly=\"Avak.StateMachine.Core.Tests.SubStateModuleOne\">\r\n  <Transition Trigger=\"EnterBbFromAa\" Target=\"Bb\" />\r\n  <Transition Trigger=\"EnterCcFromAa\" Target=\"Cc\" />\r\n</State>\r\n" +
                $"in the file File: Avak.StateMachine.Core.Tests.StateManager.XmlFilesWithSubStates.MasterStateXmlFileNoSubStateXmlFile.xml, Assembly: Avak.StateMachine.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" + Environment.NewLine +
                $"If you want to specify SubStateXml file, then ensure both the attributes are specified.";
            Assert.AreEqual(message, ex.Message);

            // Act
            // List<MasterStateBase> states = stateMachineManager.GetCurrentStateGraph().StateList;
        }

        [TestMethod]
        public void LookForSubStateXmlFileAttributeValue_NotFoundThrowsException()
        {
            // Arrange
            masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.XmlFilesWithSubStates.MasterStateXmlFileNoSubStateAssemblyValue.xml";

            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Act
            Exception ex = Assert.Throws<XmlException>(() => stateMachineManager.PopulateStateXmlFileTree());

            string message = "SubStateAssembly attribute is present, but there is no value associated with it for the state " + Environment.NewLine +
                $"<State Name=\"Aa\" SubStateAssembly=\"\" SubStateXmlFile=\"SubStateXmlFileOne.xml\">\r\n  <Transition Trigger=\"EnterBbFromAa\" Target=\"Bb\" />\r\n  <Transition Trigger=\"EnterCcFromAa\" Target=\"Cc\" />\r\n</State>\r\n" +
                $"in the file File: Avak.StateMachine.Core.Tests.StateManager.XmlFilesWithSubStates.MasterStateXmlFileNoSubStateAssemblyValue.xml, Assembly: Avak.StateMachine.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" + Environment.NewLine +
                $"Ensure correct assembly name attribute value.";
            Assert.AreEqual(message, ex.Message);

            // Act
            List<MasterStateBase> states = stateMachineManager.GetCurrentStateGraph().StateList;
        }

        [TestMethod]
        public void LookForSubStateAssemblyAttribute_NotFoundThrowsException()
        {
            // Arrange
            masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.XmlFilesWithSubStates.MasterStateXmlFileNoSubStateAssembly.xml";

            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Act
            Exception ex = Assert.Throws<XmlException>(() => stateMachineManager.PopulateStateXmlFileTree());

            string message = "SubStateXmlFile attribute is present, but SubStateAssembly attribute is not present on the state element " + Environment.NewLine +
                $"<State Name=\"Aa\" SubStateXmlFile=\"SubStateXmlFileOne.xml\">\r\n  <Transition Trigger=\"EnterBbFromAa\" Target=\"Bb\" />\r\n  <Transition Trigger=\"EnterCcFromAa\" Target=\"Cc\" />\r\n</State>\r\n" +
                $"in the file File: Avak.StateMachine.Core.Tests.StateManager.XmlFilesWithSubStates.MasterStateXmlFileNoSubStateAssembly.xml, Assembly: Avak.StateMachine.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" + Environment.NewLine +
                $"If you want to specify SubStateXml file, then ensure both the attributes are specified.";
            Assert.AreEqual(message, ex.Message);

            // Act
            List<MasterStateBase> states = stateMachineManager.GetCurrentStateGraph().StateList;
        }

        [TestMethod]
        public void LookForSubStateAssemblyAttributeValue_NotFoundThrowsException()
        {
            // Arrange
            masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager.XmlFilesWithSubStates.MasterStateXmlFileNoSubStateXmlFileValue.xml";

            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Act
            Exception ex = Assert.Throws<XmlException>(() => stateMachineManager.PopulateStateXmlFileTree());

            string message = "SubStateXmlFile attribute is present, but there is no value associated with it for the state " + Environment.NewLine +
                $"<State Name=\"Aa\" SubStateAssembly=\"Avak.StateMachine.Core.Tests.SubStateModuleOne\" SubStateXmlFile=\"\">\r\n  <Transition Trigger=\"EnterBbFromAa\" Target=\"Bb\" />\r\n  <Transition Trigger=\"EnterCcFromAa\" Target=\"Cc\" />\r\n</State>\r\n" +
                $"in the file File: Avak.StateMachine.Core.Tests.StateManager.XmlFilesWithSubStates.MasterStateXmlFileNoSubStateXmlFileValue.xml, Assembly: Avak.StateMachine.Core.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" + Environment.NewLine +
                $"Ensure correct assembly name attribute value.";
            Assert.AreEqual(message, ex.Message);

            // Act
            List<MasterStateBase> states = stateMachineManager.GetCurrentStateGraph().StateList;
        }
    }
}
