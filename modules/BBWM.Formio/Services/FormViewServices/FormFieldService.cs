using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BBWM.FormIO.Classes;
using BBWM.FormIO.Interfaces.FormViewInterfaces;
using Newtonsoft.Json.Linq;
using FieldSearcher = BBWM.FormIO.Interfaces.FormViewInterfaces.IFormFieldService.FieldSearcher;

namespace BBWM.FormIO.Services.FormViewServices;

public class FormFieldService : IFormFieldService
{
    private readonly ImmutableHashSet<string> _formCommonFields;
    private readonly ImmutableDictionary<string, FieldSearcher> _formContainerHandlers;
    private readonly ImmutableDictionary<string, FieldSearcher> _formDynamicFieldHandlers;

    public FormFieldService()
    {
        _formCommonFields = GetFormCommonFields();
        _formContainerHandlers = GetFormContainerHandlers();
        _formDynamicFieldHandlers = GetFormDynamicFieldHandlers();
    }

    /// Returns all registered form common fields.
    private static ImmutableHashSet<string> GetFormCommonFields()
    {
        return new List<string>
        {
            // Base fields.
            "textfield", "textarea", "number", "password", "checkbox", "select", "radio",
            "reviewerInput",

            // Advanced types.
            "email", "url", "phoneNumber", "tags", "address", "datetime", "time",
            "day", "currency", "file_attachments", "signature", "bodyMap", "imageUploader",

            // Data types.
            "dataMap", "editGrid",

            // Premium types.
            "custom",
        }.ToImmutableHashSet(StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));
    }

    /// Returns all registered form container fields and the functions to unwrap them.
    private ImmutableDictionary<string, FieldSearcher> GetFormContainerHandlers()
    {
        return new Dictionary<string, FieldSearcher>
            {
                // Basics types.
                { "selectBoxes", GetSelectBoxesFormFields },

                // Advanced types.
                { "survey", GetSurveyFormFields },

                // Data types.
                { "root", GetContainerFormFields },
                { "container", GetContainerFormFields }
            }
            .ToImmutableDictionary(StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));

        // Disassembles a select boxes object in a set of checkboxes.
        IEnumerable<FormField> GetSelectBoxesFormFields(JToken token, FormField? parent, string path)
        {
            if (token.SelectToken("values") is not JArray { Count: > 0 } values ||
                token.SelectToken("key")?.Value<string>() is not { Length: > 0 } key)
                return Enumerable.Empty<FormField>();

            var checkBoxes = values
                .Select(child => child.SelectToken("value")?.Value<string>())
                .Where(value => value != null)
                .Distinct()
                .Select(value => JObject.Parse($"{{\"type\":\"checkbox\", \"key\":\"{value}\"}}"));

            return checkBoxes.Select(child => new FormField { Token = child, Path = path + $".{key}" });
        }

        // Disassembles a container object into its form components.
        IEnumerable<FormField> GetContainerFormFields(JToken token, FormField? parent, string path)
        {
            if (token.SelectToken("key")?.Value<string>() is not { Length: > 0 } key)
                return Enumerable.Empty<FormField>();

            return token.Children().SelectMany(child => GetFormFields(child, parent, path + $".{key}"));
        }

        // Disassembles a survey object in a set of radio buttons.
        IEnumerable<FormField> GetSurveyFormFields(JToken token, FormField? parent, string path)
        {
            if (token.SelectToken("questions") is not JArray { Count: > 0 } questions ||
                token.SelectToken("key")?.Value<string>() is not { Length: > 0 } key)
                return Enumerable.Empty<FormField>();

            var radioButtons = questions
                .Select(child => child.SelectToken("value")?.Value<string>())
                .Where(value => value != null)
                .Distinct()
                .Select(value => JObject.Parse($"{{\"type\":\"radio\", \"key\":\"{value}\"}}"));

            return radioButtons.Select(child => new FormField { Token = child, Path = path + $".{key}" });
        }
    }

    /// Returns all registered form dynamic fields and the functions to convert them.
    private ImmutableDictionary<string, FieldSearcher> GetFormDynamicFieldHandlers()
    {
        return new Dictionary<string, FieldSearcher>
        {
            { "dataGrid", GetDataGridFormField }
        }.ToImmutableDictionary(StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));

        IEnumerable<FormField> GetDataGridFormField(JToken token, FormField? parent, string path)
        {
            if (token.SelectToken("label")?.Value<string>() is not { Length: > 0 } label ||
                token.SelectToken("type")?.Value<string>() is not { Length: > 0 } type ||
                token.SelectToken("key")?.Value<string>() is not { Length: > 0 } key)
                return Enumerable.Empty<FormField>();

            var field = new FormField
            {
                Key = key,
                Type = type,
                Label = label,
                ChildrenPath = "[*]",
                Path = path + $".{key}",
                Parent = parent,
                Token = token
            };
            field.Children = token.Children().SelectMany(child => GetFormFields(child, field, field.Path + field.ChildrenPath)).ToList();

            return new[] { field };
        }
    }

    /// Checks if given token is a registered form container.
    public bool IsFormContainer(string type)
        => _formContainerHandlers.ContainsKey(type);

    private bool IsFormContainer(JToken? token,
        [NotNullWhen(true)] out string? type)
    {
        type = token?.SelectToken("type")?.Value<string>();
        return !string.IsNullOrEmpty(type) && _formContainerHandlers.ContainsKey(type);
    }

    /// Checks if given token is a registered form dynamic field.
    public bool IsFormDynamicField(string type)
        => _formDynamicFieldHandlers.ContainsKey(type);

    private bool IsFormDynamicField(JToken? token,
        [NotNullWhen(true)] out string? key,
        [NotNullWhen(true)] out string? type)
    {
        key = token?.SelectToken("key")?.Value<string>();
        type = token?.SelectToken("type")?.Value<string>();
        return !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(key) && IsFormDynamicField(type);
    }

    /// Checks if given token is a registered form common field.
    public bool IsFormField(string type)
        => _formCommonFields.Contains(type);

    private bool IsFormField(JToken? token,
        [NotNullWhen(true)] out string? key,
        [NotNullWhen(true)] out string? type,
        [NotNullWhen(true)] out string? label)
    {
        key = token?.SelectToken("key")?.Value<string>();
        type = token?.SelectToken("type")?.Value<string>();
        label = token?.SelectToken("label")?.Value<string>();
        return !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(label) && IsFormField(type);
    }

    /// Get all the tokens in the given tree that represent inputs of a registered form type.
    public FormField? GetFormField(string? json, string path = "")
    {
        if (string.IsNullOrEmpty(json)) return null;

        // Get form field corresponding to given json code.
        var token = JObject.Parse(json);
        var root = new FormField { Path = path, Key = "root", Type = "root", Token = token };

        // Get the children of this form field recursively.
        root.Children = GetFormFields(token, root, path).ToList();

        return root;
    }

    private IEnumerable<FormField> GetFormFields(JToken? token, FormField? parent, string path = "")
    {
        if (token == null) return Enumerable.Empty<FormField>();

        // Call corresponding handling function if current token is a container.
        if (IsFormContainer(token, out var type))
            return _formContainerHandlers[type](token, parent, path);

        // If is a dynamic form field, parse it and return obtained form field.
        if (IsFormDynamicField(token, out var key, out type))
            return _formDynamicFieldHandlers[type](token, parent, path);

        // If is a common form field, return it.
        if (IsFormField(token, out key, out type, out var label))
        {
            var field = new FormField
            {
                Key = key,
                Type = type,
                Label = label,
                Path = path + $".{key}",
                Parent = parent,
                Token = token
            };
            return new List<FormField> { field };
        }

        // Otherwise, ignore this token and continue with its children.
        return token.Children().SelectMany(child => GetFormFields(child, parent, path));
    }
}