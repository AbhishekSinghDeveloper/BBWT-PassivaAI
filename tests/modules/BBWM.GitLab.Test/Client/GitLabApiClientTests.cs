using BBWM.GitLab.Client;

using Bogus;
using System.Text.Json;

using Xunit;

namespace BBWM.GitLab.Test.Client;

public class GitLabApiClientTests
{
    public GitLabApiClientTests()
    {
    }

    private static IGitLabApiClient GetService()
    {
        return new GitLabApiClient().Setup("https://gitlab.bbconsult.co.uk", "privateTokenTest", "projectId");
    }

    private static IGitLabApiClient GetServiceNotFound()
    {
        return new GitLabApiClient().Setup("https://github.com/bchavez/Bogus", "privateTokenTest", "projectId");
    }

    [Fact]
    public void Branch_Is_Exists_Test()
    {
        var service = GetService();
        var serviceNotFound = GetServiceNotFound();

        var found = service.BranchExists("testBranch");
        var notFound = serviceNotFound.BranchExists("testBranch");

        Assert.True(found);
        Assert.False(notFound);
    }

    [Fact]
    public void Create_Branch_Test()
    {
        var service = GetService();
        Action branchNotFound = () => service.CreateBranch("testBranch2", "testBranch1");

        Assert.Throws<Exception>(branchNotFound);
        Assert.NotNull(branchNotFound);
    }

    [Fact]
    public void Create_Commit_Test()
    {
        var service = GetService();

        var fakeCommitActions = new Faker<CommitAction>();
        fakeCommitActions.RuleFor(p => p.Action, s => s.Random.AlphaNumeric(7));
        fakeCommitActions.RuleFor(p => p.Content, s => s.Random.AlphaNumeric(7));
        fakeCommitActions.RuleFor(p => p.FilePath, s => s.Random.AlphaNumeric(7));

        var fakeCommit = new Faker<Commit>();
        fakeCommit.RuleFor(p => p.Branch, s => s.Random.AlphaNumeric(7));
        fakeCommit.RuleFor(p => p.CommitMessage, s => s.Random.AlphaNumeric(7));
        fakeCommit.RuleFor(p => p.CommitDescription, s => s.Random.AlphaNumeric(7));
        fakeCommit.RuleFor(p => p.Actions, s => fakeCommitActions.Generate(5));
        fakeCommit.RuleFor(p => p.AuthorName, s => s.Random.AlphaNumeric(7));
        fakeCommit.RuleFor(p => p.AuthorEmail, s => s.Internet.Email());

        Action commitNotFound = () => service.CreateCommit(fakeCommit);

        Assert.Throws<Exception>(commitNotFound);
        Assert.NotNull(commitNotFound);
    }

    [Fact]
    public void Merge_Request_Is_Exists_Test()
    {
        var service = GetService();

        Action readerException = () => service.MergeRequestExists("/testBranch1", "/testBranch2");

        Assert.Throws<JsonException>(readerException);
    }

    [Fact]
    public void Create_Merge_Request_Test()
    {
        var service = GetService();

        var fakeMergeRequest = new Faker<MergeRequest>();
        fakeMergeRequest.RuleFor(p => p.SourceBranch, s => s.Random.AlphaNumeric(7));
        fakeMergeRequest.RuleFor(p => p.TargetBranch, s => s.Random.AlphaNumeric(7));
        fakeMergeRequest.RuleFor(p => p.Title, s => s.Random.AlphaNumeric(7));
        fakeMergeRequest.RuleFor(p => p.RemoveSourceBranch, s => s.Random.Bool());

        Action mergeException = () => service.CreateMergeRequest(fakeMergeRequest.Generate());

        Assert.Throws<Exception>(mergeException);
        Assert.NotNull(mergeException);
    }

    [Fact]
    public void File_Is_Exists_Test()
    {
        var service = GetService();
        var serviceNotFound = GetServiceNotFound();

        var branchFound = service.FileExists("testBranch", "/test/path");
        var branchNotFound = serviceNotFound.FileExists("testBranch", "/test/path");

        Assert.True(branchFound);
        Assert.False(branchNotFound);
    }

    [Fact]
    public void Get_File_Test()
    {
        var service = GetService();
        var serviceNotFound = GetServiceNotFound();

        Action fileFound = () => service.GetFile("testBranch", "/test/path");
        var fileNotFound = serviceNotFound.GetFile("testBranch", "/test/path");

        Assert.Throws<JsonException>(fileFound);
        Assert.Null(fileNotFound);
    }
}