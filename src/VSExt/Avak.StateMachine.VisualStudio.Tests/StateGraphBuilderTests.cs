namespace Avak.StateMachine.VisualStudio.Tests;

using System.IO;
using System.Windows;
using System.Windows.Markup;
using Avak.StateMachine.Core;

[TestClass]
public class StateGraphBuilderTests
{
    /// <summary>Builds a graph with a labeled self-transition and a distinct source-aware transition.</summary>
    [TestMethod]
    public void Build_PreservesTransitionsAndLaysOutGraph()
    {
        const string xml = """
            <StateMachine initialState="Connected">
              <States>
                <State name="Connected">
                  <Transitions>
                    <Transition trigger="Refresh" source="UI" to="Connected" guard="Available"
                                permission="Operator" onLeaving="Close" onEntering="Prepare" onEntered="Open" />
                    <Transition trigger="Refresh" source="API" to="Disconnected" />
                  </Transitions>
                </State>
                <State name="Disconnected" />
              </States>
            </StateMachine>
            """;

        var graph = StateGraphBuilder.Build(xml);

        Assert.AreEqual(2, graph.NodeCount);
        Assert.AreEqual(2, graph.EdgeCount);
        Assert.AreEqual(3, graph.FindNode("Connected").Attr.LineWidth);
        var self = graph.Edges.Single(edge => edge.Source == "Connected" && edge.Target == "Connected");
        Assert.AreEqual(
            "Refresh\nsource: UI\nguard: Available\npermission: Operator\nonLeaving: Close\nonEntering: Prepare\nonEntered: Open",
            self.LabelText);
        Assert.IsNotNull(self.GeometryEdge.Curve);
        Assert.IsNotNull(graph.Edges.Single(edge => edge.Target == "Disconnected").GeometryEdge.Curve);
    }

    /// <summary>Invalid configuration is rejected by the framework parser before layout.</summary>
    [TestMethod]
    public void Build_InvalidTarget_ReportsFrameworkValidationError()
    {
        const string xml = """
            <StateMachine initialState="A">
              <States>
                <State name="A">
                  <Transitions><Transition trigger="Go" to="Missing" /></Transitions>
                </State>
              </States>
            </StateMachine>
            """;

        Assert.ThrowsExactly<StateMachineException>(() => StateGraphBuilder.Build(xml));
    }

    /// <summary>Layouts are produced for the repository's traffic-light and network samples.</summary>
    [TestMethod]
    [DataRow("TrafficLight.xml", 3, 3)]
    [DataRow("ConnectionWorkflow.xml", 2, 3)]
    public void Build_SampleConfiguration_ProducesLayout(string file, int nodes, int edges)
    {
        var graph = StateGraphBuilder.Build(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, file)));

        Assert.AreEqual(nodes, graph.NodeCount);
        Assert.AreEqual(edges, graph.EdgeCount);
        Assert.IsTrue(graph.Edges.All(edge => edge.GeometryEdge.Curve is not null));
    }

    [TestMethod]
    public void ViewModel_LoadXml_ExposesLayoutForRemoteGraphCanvas()
    {
        const string xml = """
            <StateMachine initialState="Ready">
              <States>
                <State name="Ready">
                  <Transitions>
                    <Transition trigger="Start" source="UI" to="Running" guard="CanStart" />
                  </Transitions>
                </State>
                <State name="Running" />
              </States>
            </StateMachine>
            """;

        var viewModel = new StateGraphViewModel(_ => Task.FromResult<string?>(null));
        viewModel.LoadXml(xml);

        Assert.AreEqual(2, viewModel.Nodes.Count);
        Assert.AreEqual(1, viewModel.Transitions.Count);
        Assert.IsTrue(viewModel.Nodes.Any(node => node.DisplayName == "▶ Ready"));
        Assert.IsTrue(viewModel.Transitions[0].DisplayLabel.Contains("source: UI", StringComparison.Ordinal));
        Assert.IsTrue(viewModel.Transitions[0].DisplayLabel.Contains("guard: CanStart", StringComparison.Ordinal));
        Assert.IsTrue(viewModel.GraphWidth > 0);
        Assert.IsTrue(viewModel.GraphHeight > 0);
        Assert.IsTrue(viewModel.StatusMessage.StartsWith("Loaded ", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ViewModel_LoadXml_InvalidConfiguration_ClearsGraphAndShowsValidationError()
    {
        const string invalidXml = """
            <StateMachine initialState="Ready">
              <States>
                <State name="Ready">
                  <Transitions><Transition trigger="Start" to="Missing" /></Transitions>
                </State>
              </States>
            </StateMachine>
            """;

        var viewModel = new StateGraphViewModel(_ => Task.FromResult<string?>(null));
        viewModel.LoadXml("""
            <StateMachine initialState="Ready">
              <States><State name="Ready" /></States>
            </StateMachine>
            """);

        viewModel.LoadXml(invalidXml);

        Assert.AreEqual(0, viewModel.Nodes.Count);
        Assert.AreEqual(0, viewModel.Transitions.Count);
        Assert.AreEqual(0, viewModel.GraphWidth);
        Assert.IsTrue(viewModel.StatusMessage.StartsWith("Invalid state-machine configuration:", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ViewModel_LoadXml_MalformedXml_ShowsParseError()
    {
        var viewModel = new StateGraphViewModel(_ => Task.FromResult<string?>(null));

        viewModel.LoadXml("<StateMachine>");

        Assert.AreEqual(0, viewModel.Nodes.Count);
        Assert.IsTrue(viewModel.StatusMessage.StartsWith("Invalid XML:", StringComparison.Ordinal));
    }

    [TestMethod]
    public void RemoteControl_Xaml_IsAValidWpfDataTemplate()
    {
        var parsedXaml = XamlReader.Parse(StateGraphControl.Xaml);

        Assert.IsInstanceOfType<DataTemplate>(parsedXaml);
    }
}
