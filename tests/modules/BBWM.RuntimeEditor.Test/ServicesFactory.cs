using BBWM.GitLab;
using BBWM.GitLab.Client;
using BBWM.RuntimeEditor.interfaces;
using BBWM.RuntimeEditor.services;
using Microsoft.Extensions.Options;

using Moq;

namespace BBWM.RuntimeEditor.Test;

public class ServicesFactory
{
    public static IEditionSendManagerService GetEditionSendManagerService()
    {
        var optionsRuntimeSettings = new Mock<IOptionsSnapshot<RuntimeEditorSettings>>();
        optionsRuntimeSettings.Setup(p => p.Value).Returns(new RuntimeEditorSettings());

        var optionsGitlab = new Mock<IOptionsSnapshot<GitLabSettings>>();
        optionsGitlab.Setup(p => p.Value).Returns(GitLab.Test.ServicesFactory.GetGitLabSettingsFake());

        return new EditionSendManagerService(
            optionsRuntimeSettings.Object,
            GetEditionSenderGitApiService(),
            GetEditionSenderAwsApiService(),
            new EditionSenderLocalFolderService());
    }

    public static IEditionSenderGitApiService GetEditionSenderGitApiService()
    {
        var optionsRuntimeSettings = new Mock<IOptionsSnapshot<RuntimeEditorSettings>>();
        optionsRuntimeSettings.Setup(p => p.Value).Returns(new RuntimeEditorSettings());

        var optionsGitlab = new Mock<IOptionsSnapshot<GitLabSettings>>();
        optionsGitlab.Setup(p => p.Value).Returns(GitLab.Test.ServicesFactory.GetGitLabSettingsFake());

        var gitlabApiClient = new Mock<IGitLabApiClient>();
        gitlabApiClient
            .Setup(p => p.CreateCommit(It.IsAny<Commit>()))
            .Returns(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        return new EditionSenderGitApiService(optionsRuntimeSettings.Object, optionsGitlab.Object, gitlabApiClient.Object);
    }

    public static IEditionSenderAwsApiService GetEditionSenderAwsApiService()
    {
        var optionsGitlab = new Mock<IOptionsSnapshot<GitLabSettings>>();
        optionsGitlab.Setup(p => p.Value).Returns(GitLab.Test.ServicesFactory.GetGitLabSettingsFake());

        var gitlabService = GitLab.Test.ServicesFactory.GetGitlabService(
            handler => GitLab.Test.ServicesFactory.GitlabCommandOKStatusHandler(handler, optionsGitlab.Object.Value, "gitpush"));

        return new EditionSenderAwsApiService(gitlabService);
    }
}
