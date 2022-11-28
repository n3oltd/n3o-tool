using System;
using System.Threading;
using System.Threading.Tasks;
namespace N3O.Tool.Utilities;

public class ServiceClient<TClient> {
    private readonly TClient _client;
    private readonly string _subscriptionId;

    public ServiceClient(TClient client, string subscriptionId) {
        _client = client;
        _subscriptionId = subscriptionId;
    }
    
    public async Task InvokeAsync(Func<TClient, Func<string, string, string, string, string, CancellationToken, Task>> resolve,
                                  CancellationToken cancellationToken = default) {
        var funcAsync = resolve(_client);

        await funcAsync("false", "false", null, null, _subscriptionId, cancellationToken);
    }
    
    public async Task InvokeAsync(Func<TClient, Func<string, string, string, string, string, string, CancellationToken, Task>> resolve,
                                  string routeParameter1,
                                  CancellationToken cancellationToken = default) {
        var funcAsync = resolve(_client);

        await funcAsync(routeParameter1, "false", "false", null, null, _subscriptionId, cancellationToken);
    }
    
    public async Task InvokeAsync<TReq>(Func<TClient, Func<string, string, string, string, string, TReq, CancellationToken, Task>> resolve,
                                        TReq req,
                                        CancellationToken cancellationToken = default) {
        var funcAsync = resolve(_client);

        await funcAsync("false", "false", null, null, _subscriptionId, req, cancellationToken);
    }
    
    public async Task<TRes> InvokeAsync<TRes>(Func<TClient, Func<string, string, string, string, string, CancellationToken, Task<TRes>>> resolve,
                                              CancellationToken cancellationToken = default) {
        var funcAsync = resolve(_client);

        var res = await funcAsync("false", "false", null, null, _subscriptionId, cancellationToken);

        return res;
    }
    
    public async Task InvokeAsync<TReq>(Func<TClient, Func<string, string, string, string, string, string, TReq, CancellationToken, Task>> resolve,
                                        string routeParameter1,
                                        TReq req,
                                        CancellationToken cancellationToken = default) {
        var funcAsync = resolve(_client);

        await funcAsync(routeParameter1, "false", "false", null, null, _subscriptionId, req, cancellationToken);
    }
    
    public async Task<TRes> InvokeAsync<TRes>(Func<TClient, Func<string, string, string, string, string, string, CancellationToken, Task<TRes>>> resolve,
                                              string routeParameter1,
                                              CancellationToken cancellationToken = default) {
        var funcAsync = resolve(_client);

        var res = await funcAsync(routeParameter1,
                                  "false",
                                  "false",
                                  null,
                                  null,
                                  _subscriptionId,
                                  cancellationToken);

        return res;
    }
    
    public async Task<TRes> InvokeAsync<TReq, TRes>(Func<TClient, Func<string, string, string, string, string, TReq, CancellationToken, Task<TRes>>> resolve,
                                                    TReq req,
                                                    CancellationToken cancellationToken = default) {
        var funcAsync = resolve(_client);

        var res = await funcAsync("false", "false", null, null, _subscriptionId, req, cancellationToken);

        return res;
    }
    
    public async Task<TRes> InvokeAsync<TReq, TRes>(Func<TClient, Func<string, string, string, string, string, string, TReq, CancellationToken, Task<TRes>>> resolve,
                                                    string routeParameter1,
                                                    TReq req,
                                                    CancellationToken cancellationToken = default) {
        var funcAsync = resolve(_client);
        
        var res = await funcAsync(routeParameter1, "false", "false", null, null, _subscriptionId, req, cancellationToken);

        return res;
    }
}