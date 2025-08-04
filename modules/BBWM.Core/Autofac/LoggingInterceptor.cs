using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using System.Reflection;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace BBWM.Core.Autofac;

public class LoggingInterceptor : IInterceptor
{
    private const string CVoidTaskResultType = "System.Threading.Tasks.VoidTaskResult";
    private readonly ILoggerFactory _loggerFactory;

    public LoggingInterceptor(ILoggerFactory loggerFactory)
        => _loggerFactory = loggerFactory;

    public void Intercept(IInvocation invocation)
    {
        var ignoreLogging = false;
        var ignoreArgs = invocation.Arguments.Length == 0;
        var ignoreReturnValue =
            invocation.Method.ReturnType == typeof(void) ||
            invocation.Method.ReturnType == typeof(Task) ||
            invocation.Method.ReturnType.FullName == CVoidTaskResultType;

        // first we get from method and then from entire class if method has not attribute
        var ignoreAttribute = invocation.MethodInvocationTarget.GetCustomAttribute<IgnoreLoggingAttribute>() ??
                                                 invocation.MethodInvocationTarget.ReflectedType.GetCustomAttribute<IgnoreLoggingAttribute>();

        if (ignoreAttribute is not null)
        {
            ignoreLogging = ignoreAttribute.IgnoreEntireLogging;
            ignoreArgs = ignoreAttribute.JustIgnoreArgumentsLogging.GetValueOrDefault();
            ignoreReturnValue = ignoreAttribute.JustIgnoreReturnValueLogging.GetValueOrDefault();
        }

        var logger = _loggerFactory.CreateLogger(invocation.MethodInvocationTarget.ReflectedType);


        if (ignoreLogging)
        {
            invocation.Proceed();
            return;
        }

        LogInvocation(logger, invocation, ignoreArgs, ignoreReturnValue);
    }

    private static void LogInvocation(ILogger logger, IInvocation invocation, bool ignoreArgs, bool ignoreReturnValue)
    {
        try
        {
            LogBefore(logger, invocation, ignoreArgs);
            invocation.Proceed();

            var returnType = invocation.Method.ReturnType;
            if (returnType == typeof(Task) || returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var task = (Task)invocation.ReturnValue;
                task.ContinueWith(t =>
                    {
                        object result = null;
                        var returnValueIsDefined = false;

                        var tType = t.GetType();
                        if (tType.IsGenericType)
                        {
                            var prop = tType.GetProperty("Result");
                            if (prop is not null && prop.PropertyType.FullName != CVoidTaskResultType)
                            {
                                result = prop.GetValue(t);
                                returnValueIsDefined = true;
                            }
                        }

                        LogAfter(logger, invocation, result, ignoreArgs, !returnValueIsDefined || ignoreReturnValue);
                    },
                    TaskContinuationOptions.ExecuteSynchronously
                )
                .ContinueWith(t =>
                {
                    t.Exception?.Handle(ex =>
                    {
                        LogException(logger, invocation, ex, ignoreArgs);
                        return true;
                    });
                },
                TaskContinuationOptions.OnlyOnFaulted);
            }
            else
            {
                LogAfter(logger, invocation, invocation.ReturnValue, ignoreArgs, ignoreReturnValue);
            }
        }
        catch (Exception e)
        {
            LogException(logger, invocation, e, ignoreArgs);
            throw;
        }
    }

    private static void LogBefore(ILogger logger, IInvocation invocation, bool ignoreArgs)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            var builder = new LogStringWithArgumentsBuilder();
            AddMethodName(invocation, builder, true);
            TryToAddMethodArguments(invocation, builder, ignoreArgs);

            builder.PerformLogging(logger.LogInformation);
        }
    }

    private static void LogAfter(ILogger logger, IInvocation invocation, object returnValue, bool ignoreArgs, bool ignoreReturnValue)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            var builder = new LogStringWithArgumentsBuilder();
            AddMethodName(invocation, builder, false);
            TryToAddMethodArguments(invocation, builder, ignoreArgs);
            TryToAddReturnValue(builder, returnValue, ignoreReturnValue);
            builder.PerformLogging(logger.LogInformation);
        }
    }

    private static void LogException(ILogger logger, IInvocation invocation, Exception e, bool ignoreArgs)
    {
        var builder = new LogStringWithArgumentsBuilder();
        AddMethodName(invocation, builder, false);
        TryToAddMethodArguments(invocation, builder, ignoreArgs);

        builder.PerformLogging((str, args) => logger.LogError(e, str, args));
    }

    private static void AddMethodName(IInvocation invocation, LogStringWithArgumentsBuilder builder, bool before)
        => builder.AddArgument((before ? "Calling" : "Called") + " method {MethodName}", $"{invocation.TargetType.Name}.{invocation.Method.Name}");

    private static void TryToAddMethodArguments(IInvocation invocation, LogStringWithArgumentsBuilder builder, bool ignoreArgs)
    {
        if (!ignoreArgs)
        {
            builder.AddArgument("with arguments {@MethodArguments}", invocation.Arguments);
        }
    }

    private static void TryToAddReturnValue(LogStringWithArgumentsBuilder builder, object returnValue, bool ignoreReturnValue)
    {
        if (!ignoreReturnValue)
        {
            builder.AddArgument("returned {@MethodReturnValue}", returnValue);
        }
    }

    private class LogStringWithArgumentsBuilder
    {
        private readonly List<string> _logSubstrings = new();
        private readonly List<object> _argsValues = new();

        public LogStringWithArgumentsBuilder AddArgument(string substringWithArgument, object argumentValue)
        {
            _logSubstrings.Add(substringWithArgument);
            _argsValues.Add(argumentValue);
            return this;
        }

        public void PerformLogging(Action<string, string[]> action)
        {
            try
            {
                action?.Invoke(string.Join(" ", _logSubstrings), _argsValues.Select(SerializeToJson).ToArray());
            }
            catch
            {
                // Ignore
            }
        }

        private static string SerializeToJson(object obj)
            => JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
    }
}