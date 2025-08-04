using BBWM.Core.Utils;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BBWM.Core.ModelHashing;


public class GlobalHashKeyJsonConverterFactory : JsonConverterFactory
{
    private readonly IModelHashingService _modelHashingService;


    public GlobalHashKeyJsonConverterFactory(IModelHashingService modelHashingService)
    {
        _modelHashingService = modelHashingService;
    }

    public override bool CanConvert(Type objectType) =>
        objectType.IsClass && (_modelHashingService.GetMaps(objectType)?.Any() ?? false);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(
            typeof(GlobalHashKeyJsonConverter<>).MakeGenericType(
                new Type[] { typeToConvert }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { _modelHashingService },
                culture: null);

    public class GlobalHashKeyJsonConverter<T> : JsonConverter<T>
    {
        private readonly IModelHashingService _modelHashingService;


        public GlobalHashKeyJsonConverter(IModelHashingService modelHashingService)
        {
            _modelHashingService = modelHashingService;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var node = JsonNode.Parse(ref reader);
            TraverseJsonObject(node, typeToConvert, ParseJsonObjectKeys);
            return node.Deserialize<T>(JsonSerializerOptionsProvider.OptionsWithoutCustomConverters);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var node = JsonSerializer.SerializeToNode(value, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters);
            TraverseJsonObject(node, value.GetType(), HashJsonObjectKeys);
            writer.WriteRawValue(node.ToString());
        }

        private static void TraverseJsonObject(JsonNode node, Type objectType, Action<JsonNode, Type> action)
        {
            foreach (var subNode in node.AsObject())
            {
                var modelProperty = objectType.GetProperty(ToUpper(subNode.Key));
                if (modelProperty is null) continue;

                if (subNode.Value is JsonArray jsonArray)
                {
                    var objects = GetCollectionItems(jsonArray, modelProperty);
                    foreach (var (key, value) in objects)
                    {
                        TraverseJsonObject(key, value, action);
                    }
                }
                else if (subNode.Value is JsonObject jsonObject)
                {
                    TraverseJsonObject(jsonObject, modelProperty.PropertyType, action);
                }
            }

            action(node, objectType);
        }

        private static IEnumerable<KeyValuePair<JsonNode, Type>> GetCollectionItems(JsonArray jsonArray, PropertyInfo modelProperty)
        {
            if (!modelProperty.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)) ||
                !modelProperty.PropertyType.IsGenericType)
                yield break;

            var childType = modelProperty.PropertyType.GetGenericArguments().FirstOrDefault();
            if (childType is null || childType.IsPrimitive) yield break;

            foreach (var child in jsonArray)
            {
                if (child is JsonObject childObject)
                {
                    yield return new KeyValuePair<JsonNode, Type>(childObject, childType);
                }
            }
        }

        private void HashJsonObjectKeys(JsonNode node, Type objectType)
        {
            var maps = _modelHashingService.GetMaps(objectType);
            foreach (var map in maps)
            {
                var jsonValue = node[map.Property] as JsonValue;

                if (jsonValue is null || !jsonValue.TryGetValue(out int val))
                    continue;

                var strVal = HashingHelper.AppendHashToKey(val, map.Salt);
                node[map.Property] = strVal;
                node[$"{map.Property}_original"] = val;
            }
        }

        private void ParseJsonObjectKeys(JsonNode node, Type objectType)
        {
            var maps = _modelHashingService.GetMaps(objectType);

            foreach (var map in maps)
            {
                var jsonValue = node[map.Property] as JsonValue;

                var modelProperty = objectType.GetProperty(ToUpper(map.Property));
                if (modelProperty is null) continue;

                var defaultValue = modelProperty.PropertyType == typeof(int) ? (int?)0 : null;

                if (jsonValue is null)
                {
                    node[map.Property] = defaultValue;
                }
                else
                {
                    var strVal = (string)jsonValue.GetValue<string>();

                    if (!string.IsNullOrEmpty(strVal))
                    {
                        var intVal = HashingHelper.GetKeyFromHashString((string)strVal, map.Salt);

                        if (intVal.HasValue)
                        {
                            node[map.Property] = intVal;
                            continue;
                        }
                    }

                    node[map.Property] = defaultValue;
                }
            }
        }

        private static string ToUpper(string name) =>
            char.ToUpperInvariant(name[0]) + name.Substring(1);
    }
}