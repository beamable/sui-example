using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Server;
using Beamable.Server.Api.RealmConfig;

namespace Beamable.SuiFederationService;

public class Configuration : IService
{
    private const string ConfigurationNamespace = "web3";
    private readonly IRealmConfigService _realmConfigService;

    public readonly string RealmSecret = Environment.GetEnvironmentVariable("SECRET") ?? "";
    private RealmConfig? _realmConfig;

    public Configuration(IRealmConfigService realmConfigService, SocketRequesterContext socketRequesterContext)
    {
        _realmConfigService = realmConfigService;

        socketRequesterContext.Subscribe<object>(Constants.Features.Services.REALM_CONFIG_UPDATE_EVENT, _ =>
        {
            BeamableLogger.Log("Realm config was updated");
            _realmConfig = null;
        });
    }

    public ValueTask<string> RPCEndpoint => GetValue(nameof(RPCEndpoint), "https://fullnode.devnet.sui.io:443");
    public ValueTask<string> SuiEnvironment => GetValue(nameof(SuiEnvironment), "devnet");
    public ValueTask<int> SuiClientTimeoutMs => GetValue(nameof(SuiClientTimeoutMs), 30000);

    private async ValueTask<T> GetValue<T>(string key, T defaultValue) where T : IConvertible
    {
        if (_realmConfig is null) _realmConfig = await _realmConfigService.GetRealmConfigSettings();

        var namespaceConfig = _realmConfig.GetValueOrDefault(ConfigurationNamespace) ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        var value = namespaceConfig.GetValueOrDefault(key);
        if (value is null)
        {
            return defaultValue;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }
}