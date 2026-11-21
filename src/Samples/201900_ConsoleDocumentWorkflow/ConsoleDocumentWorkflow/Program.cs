namespace ConsoleDocumentWorkflow;

using Avak.StateMachine.Core;

internal static class Program
{
    private static void Main()
    {
        var workflow = new DocumentWorkflow(new Document("Example document"));
        Console.WriteLine($"Initial state: {workflow.CurrentState}");

        EnsureSucceeded(workflow.Trigger("Submit", "Author", DocumentRole.Author));
        Console.WriteLine($"After submission: {workflow.CurrentState}");

        var denied = workflow.Trigger("Approve", "Reviewer", DocumentRole.Author);
        if (denied.Success || denied.Exception is not TransitionDeniedException)
        {
            throw new InvalidOperationException("Expected approval by an author to be denied.", denied.Exception);
        }
        Console.WriteLine($"Author approval denied: {denied.ErrorMessage}");
        Console.WriteLine($"After denial: {workflow.CurrentState}");

        EnsureSucceeded(workflow.Trigger("Approve", "Reviewer", DocumentRole.Reviewer));
        Console.WriteLine($"After reviewer approval: {workflow.CurrentState}");
        Console.WriteLine("Audit:");
        foreach (string entry in workflow.Document.Audit)
        {
            Console.WriteLine($"- {entry}");
        }
    }

    private static void EnsureSucceeded(TransitionResult result)
    {
        if (!result.Success)
        {
            throw new InvalidOperationException(result.ErrorMessage, result.Exception);
        }
    }
}
