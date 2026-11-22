namespace Avak.StateMachine.VisualStudio.Tests;

using System.IO;
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
}
