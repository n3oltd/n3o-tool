using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace N3O.Tool.Utilities;

public static class Json {
    public static T Deserialize<T>(string json) {
        return JsonConvert.DeserializeObject<T>(json, GetSettings());
    }

    public static string Serialize(object obj) {
        return JsonConvert.SerializeObject(obj, GetSettings());
    }

    private static JsonSerializerSettings GetSettings() {
        var settings = new JsonSerializerSettings {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        return settings;
    }
}