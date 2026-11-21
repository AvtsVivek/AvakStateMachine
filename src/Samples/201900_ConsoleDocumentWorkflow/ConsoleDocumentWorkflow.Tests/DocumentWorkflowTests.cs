namespace ConsoleDocumentWorkflow.Tests;

using Avak.StateMachine.Core;

/// <summary>Tests the document approval sample's state classes and transition rules.</summary>
/// <remarks>Each test uses a fresh in-memory document and the sample XML configuration.</remarks>
[TestClass]
public class DocumentWorkflowTests
{
    /// <summary>Initial entry records a draft, and reviewer decisions reach their corresponding final states.</summary>
    /// <param name="trigger">The review decision to make.</param>
    /// <param name="expectedState">The expected final state and audit entry.</param>
    [TestMethod]
    [DataRow("Approve", "Approved")]
    [DataRow("Reject", "Rejected")]
    public void ReviewerDecision_FromInReview_RecordsDecision(string trigger, string expectedState)
    {
        var document = new Document("Proposal");
        var workflow = new DocumentWorkflow(document);
        Assert.AreEqual("Draft", workflow.CurrentState);
        CollectionAssert.AreEqual(new[] { "Draft created" }, document.Audit.ToArray());

        Assert.IsTrue(workflow.Trigger("Submit", "Author", DocumentRole.Author).Success);
        Assert.AreEqual("InReview", workflow.CurrentState);
        Assert.IsTrue(workflow.Trigger(trigger, "Reviewer", DocumentRole.Reviewer).Success);

        Assert.AreEqual(expectedState, workflow.CurrentState);
        CollectionAssert.AreEqual(
            new[] { "Draft created", "Submitted for review", expectedState }, document.Audit.ToArray());
    }

    /// <summary>Draft's state check denies submission until the document contains non-whitespace content.</summary>
    /// <remarks>The failed attempt preserves Draft and its audit; adding content allows a subsequent submission.</remarks>
    [TestMethod]
    public void Submit_WithoutContent_IsDeniedUntilContentIsProvided()
    {
        var document = new Document(" ");
        var workflow = new DocumentWorkflow(document);

        var result = workflow.Trigger("Submit", "Author", DocumentRole.Author);

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<TransitionDeniedException>(result.Exception);
        Assert.AreEqual("Draft", workflow.CurrentState);
        CollectionAssert.AreEqual(new[] { "Draft created" }, document.Audit.ToArray());
        document.Content = "Proposal";
        Assert.IsTrue(workflow.Trigger("Submit", "Author", DocumentRole.Author).Success);
        Assert.AreEqual("InReview", workflow.CurrentState);
        CollectionAssert.AreEqual(new[] { "Draft created", "Submitted for review" }, document.Audit.ToArray());
    }

    /// <summary>A non-reviewer cannot approve or reject, even when using the reviewer trigger source.</summary>
    /// <param name="trigger">The protected review decision to attempt.</param>
    [TestMethod]
    [DataRow("Approve")]
    [DataRow("Reject")]
    public void ReviewerDecision_ByAuthor_IsDeniedAndPreservesReviewState(string trigger)
    {
        var document = new Document("Proposal");
        var workflow = new DocumentWorkflow(document);
        Assert.IsTrue(workflow.Trigger("Submit", "Author", DocumentRole.Author).Success);

        var result = workflow.Trigger(trigger, "Reviewer", DocumentRole.Author);

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType<TransitionDeniedException>(result.Exception);
        Assert.AreEqual("InReview", workflow.CurrentState);
        CollectionAssert.AreEqual(new[] { "Draft created", "Submitted for review" }, document.Audit.ToArray());
    }

    /// <summary>A correct role does not bypass the trigger's configured source filter.</summary>
    /// <remarks>The attempt uses the Author source for a reviewer decision and leaves the document in review.</remarks>
    [TestMethod]
    public void Approve_WithWrongSource_IsRejectedWithoutChangingState()
    {
        var document = new Document("Proposal");
        var workflow = new DocumentWorkflow(document);
        Assert.IsTrue(workflow.Trigger("Submit", "Author", DocumentRole.Author).Success);

        var result = workflow.Trigger("Approve", "Author", DocumentRole.Reviewer);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("InReview", workflow.CurrentState);
        CollectionAssert.AreEqual(new[] { "Draft created", "Submitted for review" }, document.Audit.ToArray());
    }

    /// <summary>Separate workflows maintain independent document state and in-memory audit histories.</summary>
    [TestMethod]
    public void SeparateWorkflows_DoNotShareStateOrAudit()
    {
        var first = new DocumentWorkflow(new Document("First"));
        var second = new DocumentWorkflow(new Document("Second"));

        Assert.IsTrue(first.Trigger("Submit", "Author", DocumentRole.Author).Success);

        Assert.AreEqual("InReview", first.CurrentState);
        Assert.AreEqual("Draft", second.CurrentState);
        CollectionAssert.AreEqual(new[] { "Draft created", "Submitted for review" }, first.Document.Audit.ToArray());
        CollectionAssert.AreEqual(new[] { "Draft created" }, second.Document.Audit.ToArray());
    }
}
