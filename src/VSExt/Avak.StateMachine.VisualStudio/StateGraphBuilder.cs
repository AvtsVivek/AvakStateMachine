namespace Avak.StateMachine.VisualStudio;

using System;
using System.Collections.Generic;
using Avak.StateMachine.Core.Configuration;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Miscellaneous;

internal static class StateGraphBuilder
{
    public static Graph Build(string xml)
    {
        var configuration = new XmlConfigurationLoader().LoadFromXml(xml);
        var graph = new Graph();

        foreach (var state in configuration.States.Values)
        {
            var node = graph.AddNode(state.Name);
            node.LabelText = state.Name;
            if (state.Name == configuration.InitialState)
            {
                node.Attr.LineWidth = 3;
            }
        }

        foreach (var state in configuration.States.Values)
        {
            foreach (var (trigger, transitions) in state.Transitions)
            {
                foreach (var transition in transitions)
                {
                    graph.AddEdge(state.Name, Label(trigger, transition), transition.ToState);
                }
            }
        }

        graph.CreateGeometryGraph();
        foreach (var node in graph.Nodes)
        {
            double width = Math.Max(100, (node.LabelText.Length * 8) + 32);
            node.GeometryNode.BoundaryCurve = CurveFactory.CreateRectangle(width, 44, new Point());
        }

        LayoutHelpers.CalculateLayout(graph.GeometryGraph, graph.LayoutAlgorithmSettings, null);
        return graph;
    }

    private static string Label(string trigger, TransitionConfiguration transition)
    {
        var parts = new List<string> { trigger };
        Add(parts, "source", transition.Source);
        Add(parts, "guard", transition.GuardName);
        Add(parts, "permission", transition.PermissionName);
        Add(parts, "onLeaving", transition.OnLeavingCallbackName);
        Add(parts, "onEntering", transition.OnEnteringCallbackName);
        Add(parts, "onEntered", transition.OnEnteredCallbackName);
        return string.Join("\n", parts);
    }

    private static void Add(List<string> parts, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            parts.Add($"{name}: {value}");
        }
    }
}
