using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.SuiFederationService;
using SuiFederationService.Features.Wallets;
using Swan;

namespace SuiFederationService.Features.Sui;

public class SuiCliService : IService
{
    private readonly Configuration _configuration;
    private readonly WalletService _walletService;
    private const string LogsPattern = @"\[[^a-zA-Z]+\] .+";

    private bool _initialized;
    private string? _suiPath;
    private string? _suiConfigPath;

    public SuiCliService(Configuration configuration, WalletService walletService)
    {
        _configuration = configuration;
        _walletService = walletService;
    }

    public async Task Initialize()
    {
        _suiPath = GetSuiPath();

        var realmWallet = await _walletService.GetOrCreateRealmWallet();
        
        _suiConfigPath = Path.Combine(Path.GetTempPath(), "sui_client.yaml");
        var suiConfig = GetSuiClientConfig(realmWallet.Address, realmWallet.PrivateKeyBytes.ToBase64(), await _configuration.SuiEnvironment, await _configuration.RPCEndpoint);
        await File.WriteAllTextAsync(_suiConfigPath, suiConfig);

        _initialized = true;
    }

    public async Task<string> ExecuteClient(string args)
    {
        CheckInitialized();
        using var process = new Process();
        process.StartInfo =
            new ProcessStartInfo(_suiPath!, $"client {args} --client.config {_suiConfigPath} --json")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

        process.Start();

        var outputText = await process.StandardOutput.ReadToEndAsync();
        var outputError = await process.StandardError.ReadToEndAsync();

        process.WaitForExit(await _configuration.SuiClientTimeoutMs);

        if (!string.IsNullOrEmpty(outputError))
        {
            BeamableLogger.LogError("Process error: {error}", outputError);
            throw new SuiCliException(outputError);
        }

        BeamableLogger.Log("Process output: {output}", outputText);

        return Regex.Replace(outputText, LogsPattern, "");
    }

    private void CheckInitialized()
    {
        if (!_initialized)
            throw new SuiCliException("Sui client is not initialized");
    }

    private string GetSuiClientConfig(string activeAddress, string privateKeyBase64, string activeEnv, string rpc)
    {
        return @$"---
keystore:
  InMem: 
    keys: 
      ""{activeAddress}"": ""{privateKeyBase64}""
envs:
  - alias: {activeEnv}
    rpc: ""{rpc}""
    ws: ~
active_env: {activeEnv}
active_address: ""{activeAddress}""";
    }

    private string GetSuiPath()
    {
        if (Environment.GetEnvironmentVariable("SUI_PATH") is { } path)
        {
            return path;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.cargo/bin/sui.exe";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "/usr/local/bin/sui";
        }

        throw new SuiCliException("Sui path not defined for the current OS platform");
    }
}

public class SuiCliException : Exception
{
    public SuiCliException(string? message) : base(message)
    {
    }
}