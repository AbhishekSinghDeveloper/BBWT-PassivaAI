
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace BBWM.AWS.EventBridge.Test;

// https://docs.microsoft.com/en-us/dotnet/core/testing/order-unit-tests?pivots=xunit#order-by-custom-attribute
public class TestPriorityOrderer : ITestCaseOrderer
{
    public const string TYPE_NAME = "BBWM.AWS.EventBridge.Test.TestPriorityOrderer";
    public const string ASSEMBLY_NAME = "BBWT.Tests";

    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(
    IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        var assemblyName = typeof(TestPriorityAttribute).AssemblyQualifiedName!;
        var sortedMethods = new SortedDictionary<int, List<TTestCase>>();
        foreach (var testCase in testCases)
        {
            var priority = testCase.TestMethod.Method
                .GetCustomAttributes(assemblyName)
                .FirstOrDefault()
                ?.GetNamedArgument<int>(nameof(TestPriorityAttribute.Priority)) ?? 0;

            GetOrCreate(sortedMethods, priority).Add(testCase);
        }

        foreach (var testCase in
            sortedMethods.Keys.SelectMany(
                priority => sortedMethods[priority].OrderBy(
                    testCase => testCase.TestMethod.Method.Name)))
        {
            yield return testCase;
        }
    }

    private static TValue GetOrCreate<TKey, TValue>(
        IDictionary<TKey, TValue> dictionary, TKey key)
        where TKey : struct
        where TValue : new() =>
        dictionary.TryGetValue(key, out var result)
            ? result
            : (dictionary[key] = new TValue());
}
