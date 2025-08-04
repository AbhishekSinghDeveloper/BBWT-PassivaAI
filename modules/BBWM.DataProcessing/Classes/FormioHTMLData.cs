using System.Text.Json.Nodes;

namespace BBWM.DataProcessing.Classes
{
    public class FormioHTMLData
    {
        public string HtmlContent { get; set; }
        public JsonObject FormData { get; set; }
    }
}
