using System.Dynamic;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BBWM.FormIO.Utils;

public partial class FormDataJsonParser
{
    [JsonProperty("data")]
    public Data Data { get; set; }
}

public partial class Data
{
    [JsonProperty("file_attachments")]
    public FileAttachments[]? FileAttachments { get; set; }

    [JsonIgnore]
    public List<KeyValuePair<string, ImageUploader>>? ImageUploader { get; set; }

    [JsonProperty("submit")]
    public bool Submit { get; set; }
}

public partial class FileAttachments
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("thumbnailKey")]
    public string ThumbnailKey { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("thumbnailUrl")]
    public string ThumbnailUrl { get; set; }

    [JsonProperty("isImage")]
    public bool IsImage { get; set; }

    [JsonProperty("fileName")]
    public string FileName { get; set; }

    [JsonProperty("size")]
    public long Size { get; set; }

    [JsonProperty("uploadTime")]
    public DateTimeOffset UploadTime { get; set; }

    [JsonProperty("lastUpdated")]
    public DateTimeOffset LastUpdated { get; set; }

    [JsonProperty("id_original")]
    public long IdOriginal { get; set; }

    [JsonProperty("originalName")]
    public string OriginalName { get; set; }

    [JsonProperty("hash")]
    public string Hash { get; set; }
}

public partial class ImageUploader
{
    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("fileName")]
    public string FileName { get; set; }

    [JsonProperty("uploadTime")]
    public DateTimeOffset UploadTime { get; set; }
}

public partial class FormDataJsonParser
{
    public static FormDataJsonParser FromJson(string json) {
        var result = JsonConvert.DeserializeObject<FormDataJsonParser>(json, Converter.Settings)!;
        if (result == null || result.Data == null)
        {
            return result;
        }
        try
        {
            dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            var otherObjects = data.data as IDictionary<string, object>;
            result.Data.ImageUploader = new List<KeyValuePair<string, ImageUploader>>();
            foreach (var item in otherObjects)
            {
                if (item.Value is System.Dynamic.ExpandoObject)
                {
                    if (((IDictionary<string, object>)item.Value).ContainsKey("thumbnailUrl"))
                    {

                    }
                    else if (((IDictionary<string, object>)item.Value).ContainsKey("url"))
                    {
                        string jsonString = JsonConvert.SerializeObject(item.Value);
                        result?.Data?.ImageUploader?.Add(new KeyValuePair<string, ImageUploader>(item.Key, JsonConvert.DeserializeObject<ImageUploader>(jsonString)));
                    }
                }
            }
        }
        catch (Exception)
        {
            
        }
        return result;
    }
}

public static class Serialize
{
    public static string ToJson(this FormDataJsonParser self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
        {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        }
    };
}