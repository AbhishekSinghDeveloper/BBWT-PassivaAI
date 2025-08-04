using BBWM.Core.Autofac;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace BBWM.Core.Test.Autofac;

// TODO: It's just a dummy test that doesn't actually test anything.
// I suppose there are should be multiple check for all possible cases with real methods used as IInvocation.
public class LoggingInterceptorTest
{
    public LoggingInterceptorTest()
    {
    }

    private static LoggingInterceptor GetService()
    {
        var loggerFactory = new Mock<ILoggerFactory>();

        return new LoggingInterceptor(loggerFactory.Object);
    }

    [Fact]
    public void Interception_Test()
    {
        var service = GetService();

        var invocation = new Mock<Castle.DynamicProxy.IInvocation>();

        invocation.Object.ReturnValue = "test";
        invocation.Setup(p => p.MethodInvocationTarget.ReturnType).Returns(typeof(IgnoreLoggingAttribute));
        invocation.Setup(p => p.Method.ReturnType.FullName).Returns("System.Threading.Tasks.VoidTaskResult");

        Action result = () => service.Intercept(invocation.Object);

        Assert.Throws<InvalidCastException>(result);
    }
}
