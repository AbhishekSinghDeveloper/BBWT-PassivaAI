using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BBWM.FormIO.Extensions
{
    public static class FormJSONExtentions
    {
        public static List<JToken> GetInnerFormDefinitionComponents(this JToken? root, Func<JToken, bool> query)
        {
            var result = new List<JToken>();
            if (root != null)
            {
                var childrens = root.Children().Where(x => x is not JProperty);
                result.AddRange(childrens.Where(query));
                if (childrens.Any())
                {
                    foreach (var item in childrens)
                    {
                        var components = item["components"] ?? item["columns"];
                        if (components != null)
                        {
                            result.AddRange(GetInnerFormDefinitionComponents(components, query));
                        }

                        var table = item["rows"];
                        if(table != null)
                        {
                            foreach (var row in table)
                            {
                                result.AddRange(GetInnerFormDefinitionComponents(row, query));
                            }
                        }   
                    }
                }
            }
            return result;
        }

        public static List<JArray> GetInnerFormDataValues(this JToken? root, Func<JProperty, bool> query)
        {
            var result = new List<JArray>();
            if (root != null)
            {
                var props = root.Children<JProperty>().Where(query).Select(x => x.First as JArray).ToList();
                props.ForEach(x =>
                {
                    if (x != null)
                    {
                        result.Add(x);
                    }
                });
            }
            return result;
        }
    }
}
