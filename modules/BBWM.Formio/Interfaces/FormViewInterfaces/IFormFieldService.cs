using BBWM.FormIO.Classes;
using Newtonsoft.Json.Linq;

namespace BBWM.FormIO.Interfaces.FormViewInterfaces;

public interface IFormFieldService
{
    delegate IEnumerable<FormField> FieldSearcher(JToken token, FormField? father, string path);

    bool IsFormField(string type);
    bool IsFormContainer(string type);
    bool IsFormDynamicField(string type);
    FormField? GetFormField(string? json, string path = "");
}