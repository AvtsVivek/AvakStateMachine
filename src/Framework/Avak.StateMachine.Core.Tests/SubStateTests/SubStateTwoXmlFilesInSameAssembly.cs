using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.SubStateTests
{
    [TestClass]
    [DoNotParallelize]
    public class SubStateTwoXmlFilesInSameAssembly
    {
        private string masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager." +
            "XmlFilesWithSubStates.MasterStateXmlFileTwoXmlFilesInSameAssembly.xml";
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
        public void TwoXmlFilesInSameAssembly_ThrowExceptioni()
        {
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Act
            Exception ex = Assert.Throws<Exception>(() => stateMachineManager.PopulateStateXmlFileTree());

            string message = $"StateXmlFileTree already contains an xmlFile from the same assembly: " + Environment.NewLine +
                $"File: Avak.StateMachine.Core.Tests.SSM_UT11_L2_1.StateManager.SubStateXmlFileL2_2.xml, Assembly: Avak.StateMachine.Core.Tests.SSM_UT11_L2_1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" + Environment.NewLine +
                $"Existing xmlFile: " + Environment.NewLine +
                $"File: Avak.StateMachine.Core.Tests.SSM_UT11_L2_1.StateManager.SubStateXmlFileL2_1.xml, Assembly: Avak.StateMachine.Core.Tests.SSM_UT11_L2_1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" + Environment.NewLine +
                $"Existing xmlFile Level: 2, New xmlFile Level: 2" + Environment.NewLine +
                $"You are trying to add xml file at Level: 2 whose assembly is already added at level: 2";

            Assert.AreEqual(message, ex.Message);
        }
    }
}
