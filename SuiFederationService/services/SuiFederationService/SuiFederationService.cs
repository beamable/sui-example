using System;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Server;
using Nethereum.Hex.HexConvertors.Extensions;
using SuiFederationService.Features;
using SuiFederationService.Features.Sui;
using SuiFederationService.Features.Wallets;
using Swan;

namespace Beamable.SuiFederationService
{
	[Microservice("SuiFederationService")]
	public class SuiFederationService : Microservice
	{
		private readonly WalletService _walletService;

		[ConfigureServices]
		public static void Configure(IServiceBuilder serviceBuilder)
		{
			var dependencyBuilder = serviceBuilder.Builder;
			dependencyBuilder.AddFeatures();
		}
		
		[InitializeServices]
		public static async Task Initialize(IServiceInitializer initializer)
		{
			try
			{
				await initializer.Provider.GetService<SuiCliService>().Initialize();
			}
			catch (Exception ex)
			{
				BeamableLogger.LogException(ex);
				BeamableLogger.LogError("Service initialization failed. Fix the issues before using the service.");
			}
		}

		public SuiFederationService(WalletService walletService)
		{
			_walletService = walletService;
		}

		[AdminOnlyCallable("sui/export-realm-wallet")]
		public async Task<string> ExportRealmWallet()
		{
			var wallet = await _walletService.GetOrCreateRealmWallet();
			return wallet.ExpandedPrivateKey.ToHex();
		}
	}
}
