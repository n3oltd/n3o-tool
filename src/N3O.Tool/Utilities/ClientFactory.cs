using Flurl;
using System;
using System.Net.Http;

namespace N3O.Tool.Utilities;

public class ClientFactory<T> {
    private const string BaseUrl = nameof(BaseUrl);

    public ServiceClient<T> Create(string subscriptionId) {
        var httpClient = new HttpClient();
        var client = (T) Activator.CreateInstance(typeof(T), httpClient);

        var baseUrlProperty = client.GetType().GetProperty(BaseUrl);
        var baseUrlValue = new Url("https://n3o.cloud").AppendPathSegment(baseUrlProperty.GetValue(client))
                                                       .ToString();

        baseUrlProperty.SetValue(client, baseUrlValue);
        
        return new ServiceClient<T>(client, subscriptionId);
    }
}