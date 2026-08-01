using Avak.StateMachine.Core.Contracts;
using Avak.StateMachine.Core.Implimentation;
using System.Reflection;

namespace Avak.StateMachine.Core.Tests.SubStateTests
{
    // The same xml file, first referenced at level 2 is referenced again at level 4.
    // The second time it is referenced, it is at a different level, level 4. This should throw an exception.
    [TestClass]
    [DoNotParallelize]
    public class SubStateDuplicateXmlFile
    {
        private string masterStateXmlFile = "Avak.StateMachine.Core.Tests.StateManager." +
            "XmlFilesWithSubStates.MasterStateXmlFileDuplicateSubStateXmlFile.xml";
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
        public void SameXmlFileReferencedSecondTime_ThrowExceptioni()
        {
            IXmlKeys constants = new XmlKeys();

            StateMachineManager stateMachineManager = new(constants,
                StateDependencyImplimentation.StateDependencyTypeFinderDefaultImplimentation,
                StateDependencyImplimentation.StateDependencyResolverDefaultImplimentation);

            stateMachineManager.SetMasterStateFile(Assembly.GetExecutingAssembly(), masterStateXmlFile);

            // Act
            Exception ex = Assert.Throws<Exception>(() => stateMachineManager.PopulateStateXmlFileTree());

            string message = $"StateXmlFileTree already contains the specified xmlFile: " + Environment.NewLine +
                $"File: Avak.StateMachine.Core.Tests.SSM_UT12_L2_2.StateManager.SubStateXmlFileL2_2.xml, Assembly: Avak.StateMachine.Core.Tests.SSM_UT12_L2_2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" + Environment.NewLine +
                $"Existing xmlFile: " + Environment.NewLine +
                $"File: Avak.StateMachine.Core.Tests.SSM_UT12_L2_2.StateManager.SubStateXmlFileL2_2.xml, Assembly: Avak.StateMachine.Core.Tests.SSM_UT12_L2_2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" + Environment.NewLine +
                $"Existing xmlFile Level: 2, New xmlFile Level: 4" + Environment.NewLine +
                $"You are trying to add and the same xml file at Level: 4 which is already added at level: 2";

            Assert.AreEqual(message, ex.Message);
        }
    }
}
