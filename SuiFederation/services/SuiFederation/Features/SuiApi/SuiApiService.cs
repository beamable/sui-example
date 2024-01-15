using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jering.Javascript.NodeJS;
using Beamable.Common;
using Beamable.SuiFederation.Features.Minting.Models;
using Beamable.SuiFederation.Features.SuiApi.Exceptions;
using Beamable.SuiFederation.Features.SuiApi.Models;
using Newtonsoft.Json;
using SuiFederationCommon.Models;

namespace Beamable.SuiFederation.Features.SuiApi;

public class SuiApiService : IService
{
    private const string BridgeModulePath = "./js/bridge.js";
    private readonly Configuration _configuration;

    public SuiApiService(Configuration configuration)
    {
        _configuration = configuration;
    }

    public static async Task<SuiKeys> ExportPrivateKey()
    {
        using (new Measure($"Sui.exportPrivateKey"))
        {
            try
            {
                var response = await StaticNodeJSService.InvokeFromFileAsync<string>(
                    BridgeModulePath,
                    "exportSecret");
                return JsonConvert.DeserializeObject<SuiKeys>(response);
            }
            catch (Exception ex)
            {
                BeamableLogger.LogWarning("Can't generate new private key. Error: {error}", ex.Message);
                throw new SuiApiException(ex.Message);
            }
        }
    }

    public async Task<bool> VerifySignature(string token, string challenge, string solution)
    {
        using (new Measure($"Sui.verifySignature: {token}"))
        {
            try
            {
                return await StaticNodeJSService.InvokeFromFileAsync<bool>(
                    BridgeModulePath,
                    "verifySignature",
                    new object[] { token, challenge, solution });
            }
            catch (Exception ex)
            {
                BeamableLogger.LogWarning("Can't verify signature for {token}. Error: {error}", token, ex.Message);
                throw new SuiApiException(ex.Message);
            }
        }
    }

    public async Task<SuiBalance> GetBalance(string address, string[] coinModules)
    {
        using (new Measure($"Sui.GetBalance: {address} for -> {string.Join(',', coinModules)}"))
        {
            try
            {
                var environment = await _configuration.SuiEnvironment;
                var packageId = await _configuration.PackageId;
                var response = await StaticNodeJSService.InvokeFromFileAsync<string>(
                    BridgeModulePath,
                "getBalance",
                    new object[] { address, packageId, coinModules, environment });
                return JsonConvert.DeserializeObject<SuiBalance>(response);
            }
            catch (Exception ex)
            {
                BeamableLogger.LogWarning("Can't get balance for {address}. Error: {error}", address, ex.Message);
                throw new SuiApiException(ex.Message);
            }
        }
    }

    public async Task<IEnumerable<SuiObject>> GetOwnedObjects(string address)
    {
        using (new Measure($"Sui.GetOwnedObjects: {address}"))
        {
            try
            {
                var environment = await _configuration.SuiEnvironment;
                var packageId = await _configuration.PackageId;
                var result = await StaticNodeJSService.InvokeFromFileAsync<string>(
                    BridgeModulePath,
                    "getOwnedObjects",
                    new object[] { address, packageId, environment });

                return JsonConvert.DeserializeObject<IEnumerable<SuiObject>>(result);
            }
            catch (Exception ex)
            {
                BeamableLogger.LogWarning("Can't get objects for {address}. Error: {error}", address, ex.Message);
                throw new SuiApiException(ex.Message);
            }
        }
    }

    public async Task<SuiCapObjects> InitializeObjects()
    {
        using (new Measure("Sui.Initialize"))
        {
            try
            {
                var environment = await _configuration.SuiEnvironment;
                var packageId = await _configuration.PackageId;
                var secretKey = await _configuration.PrivateKey;
                var result = await StaticNodeJSService.InvokeFromFileAsync<string>(
                    BridgeModulePath,
                    "getCapObjects",
                    new object[] { secretKey, packageId, environment });

                return JsonConvert.DeserializeObject<SuiCapObjects>(result);
            }
            catch (Exception ex)
            {
                BeamableLogger.LogWarning("Can't get cap objects. Error: {error}", ex.Message);
                throw new SuiApiException(ex.Message);
            }
        }
    }

    public async Task<SuiTransactionResult> MintInventoryItems(string token, InventoryMintRequest request)
    {
        using (new Measure($"Sui.MintInventoryItems: for {token} -> {string.Join(',', request.GameItems.Select(x => x.Name))} -> Currency: {string.Join(',', request.CurrencyItems.Select(x => x.Name))}"))
        {
            try
            {
                var environment = await _configuration.SuiEnvironment;
                var packageId = await _configuration.PackageId;
                var secretKey = await _configuration.PrivateKey;
                var mintRequestJson = JsonConvert.SerializeObject(request);
                var result =  await StaticNodeJSService.InvokeFromFileAsync<string>(
                    BridgeModulePath,
                    "mintInventory",
                    new object[] { packageId, token, mintRequestJson, secretKey, environment });
                return JsonConvert.DeserializeObject<SuiTransactionResult>(result);;
            }
            catch (Exception ex)
            {
                BeamableLogger.LogWarning("Can't mint items for {token}. Error: {error}", token, ex.Message);
                throw new SuiApiException(ex.Message);
            }
        }
    }
}