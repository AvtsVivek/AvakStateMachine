namespace Avak.StateMachine.VisualStudio;

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avak.StateMachine.Core;
using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Drawing;

[DataContract]
public sealed class StateGraphViewModel : NotifyPropertyChangedObject
{
    private const double CanvasPadding = 32;
    private string xmlPath = string.Empty;
    private string statusMessage = "Enter the path to a state-machine XML file.";
    private double graphWidth;
    private double graphHeight;
    private readonly Func<CancellationToken, Task<string?>> selectXmlFileAsync;

    public StateGraphViewModel(Func<CancellationToken, Task<string?>> selectXmlFileAsync)
    {
        this.selectXmlFileAsync = selectXmlFileAsync;
        this.LoadCommand = new AsyncCommand((_, cancellationToken) => this.LoadGraphAsync(cancellationToken));
        this.BrowseCommand = new AsyncCommand((_, cancellationToken) => this.BrowseForXmlFileAsync(cancellationToken));
    }

    [DataMember]
    public string XmlPath
    {
        get => this.xmlPath;
        set => this.SetProperty(ref this.xmlPath, value);
    }

    [DataMember]
    public string StatusMessage
    {
        get => this.statusMessage;
        private set => this.SetProperty(ref this.statusMessage, value);
    }

    [DataMember]
    public double GraphWidth
    {
        get => this.graphWidth;
        private set => this.SetProperty(ref this.graphWidth, value);
    }

    [DataMember]
    public double GraphHeight
    {
        get => this.graphHeight;
        private set => this.SetProperty(ref this.graphHeight, value);
    }

    [DataMember]
    public ObservableList<StateGraphNodeViewModel> Nodes { get; } = new();

    [DataMember]
    public ObservableList<StateGraphTransitionViewModel> Transitions { get; } = new();

    [DataMember]
    public IAsyncCommand LoadCommand { get; }

    [DataMember]
    public IAsyncCommand BrowseCommand { get; }

    public async Task BrowseForXmlFileAsync(CancellationToken cancellationToken)
    {
        var selectedPath = await this.selectXmlFileAsync(cancellationToken);
        if (selectedPath is not null)
        {
            this.XmlPath = selectedPath;
            await this.LoadGraphAsync(cancellationToken);
        }
    }

    public async Task LoadGraphAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(this.XmlPath))
        {
            this.ClearGraph("Enter the path to a state-machine XML file.");
            return;
        }

        try
        {
            var xml = await File.ReadAllTextAsync(this.XmlPath, cancellationToken);
            this.LoadXml(xml);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            this.ClearGraph($"Could not read the XML file: {exception.Message}");
        }
    }

    internal void LoadXml(string xml)
    {
        try
        {
            var graph = StateGraphBuilder.Build(xml);
            this.SetGraph(graph);
            this.StatusMessage = $"Loaded {graph.NodeCount} states and {graph.EdgeCount} transitions.";
        }
        catch (StateMachineException exception) when (exception.InnerException is System.Xml.XmlException xmlException)
        {
            this.ClearGraph($"Invalid XML: {xmlException.Message}");
        }
        catch (StateMachineException exception)
        {
            this.ClearGraph($"Invalid state-machine configuration: {exception.Message}");
        }
    }

    private void SetGraph(Graph graph)
    {
        this.Nodes.RemoveRange(0, this.Nodes.Count);
        this.Transitions.RemoveRange(0, this.Transitions.Count);

        var bounds = graph.GeometryGraph.BoundingBox;
        var nodesByName = graph.Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        var initialState = GetInitialState(graph);
        foreach (var node in graph.Nodes)
        {
            var nodeBounds = node.GeometryNode.BoundingBox;
            double width = nodeBounds.Width;
            double height = nodeBounds.Height;
            double x = node.GeometryNode.Center.X - bounds.Left + CanvasPadding - (width / 2);
            double y = bounds.Top - node.GeometryNode.Center.Y + CanvasPadding - (height / 2);
            var displayName = node.Id == initialState ? $"▶ {node.LabelText}" : node.LabelText;
            this.Nodes.Add(new StateGraphNodeViewModel(displayName, x, y, width, height));
        }

        var seenByPair = new System.Collections.Generic.Dictionary<(string Source, string Target), int>();
        foreach (var edge in graph.Edges)
        {
            var pair = (edge.Source, edge.Target);
            seenByPair.TryGetValue(pair, out int seen);
            seenByPair[pair] = seen + 1;

            var source = nodesByName[edge.Source].GeometryNode.Center;
            var target = nodesByName[edge.Target].GeometryNode.Center;
            double x = ((source.X + target.X) / 2) - bounds.Left + CanvasPadding;
            double y = bounds.Top - ((source.Y + target.Y) / 2) + CanvasPadding;
            if (pair.Source == pair.Target)
            {
                y -= 28;
            }

            y += seen * 18;
            string label = edge.LabelText.Replace("\n", " · ", StringComparison.Ordinal);
            this.Transitions.Add(new StateGraphTransitionViewModel(
                $"→ {label}",
                Math.Max(0, x - 48),
                Math.Max(0, y - 12)));
        }

        this.GraphWidth = Math.Ceiling(bounds.Width + (CanvasPadding * 2));
        this.GraphHeight = Math.Ceiling(bounds.Height + (CanvasPadding * 2));
    }

    private static string GetInitialState(Graph graph) =>
        graph.Nodes.Single(node => node.Attr.LineWidth == 3).Id;

    private void ClearGraph(string message)
    {
        this.Nodes.RemoveRange(0, this.Nodes.Count);
        this.Transitions.RemoveRange(0, this.Transitions.Count);
        this.GraphWidth = 0;
        this.GraphHeight = 0;
        this.StatusMessage = message;
    }
}

[DataContract]
public sealed class StateGraphNodeViewModel
{
    public StateGraphNodeViewModel(string displayName, double x, double y, double width, double height)
    {
        this.DisplayName = displayName;
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
    }

    [DataMember]
    public string DisplayName { get; }

    [DataMember]
    public double X { get; }

    [DataMember]
    public double Y { get; }

    [DataMember]
    public double Width { get; }

    [DataMember]
    public double Height { get; }
}

[DataContract]
public sealed class StateGraphTransitionViewModel
{
    public StateGraphTransitionViewModel(string displayLabel, double x, double y)
    {
        this.DisplayLabel = displayLabel;
        this.X = x;
        this.Y = y;
    }

    [DataMember]
    public string DisplayLabel { get; }

    [DataMember]
    public double X { get; }

    [DataMember]
    public double Y { get; }
}
