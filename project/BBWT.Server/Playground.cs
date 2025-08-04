/// <summary>
/// This class is supposed to be a testing playground, so you can add temporary code to test particular parts of the system,
/// avoiding making changes in the core code parts.
/// In particular, you can mark this file as ignored to be committed, allowing to keep your test code that won't go to the repo.
/// For example you can test a function triggered after the application is configured on start up and just before it runs.
/// </summary>
internal static class Playground
{
    internal static void OnAppRun(WebApplication app)
    {
    }

    internal static void OnAppException(Exception ex)
    {

    }
}