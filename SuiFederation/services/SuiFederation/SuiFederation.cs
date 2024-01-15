using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Api.Inventory;
using Beamable.Server;
using Beamable.SuiFederation.Features.Minting;
using Beamable.SuiFederation.Features.Transactions;
using SuiFederation.Features.Accounts.Exceptions;
using SuiFederation.Features.Wallets;
using SuiFederationCommon;

namespace Beamable.SuiFederation
{
	[Microservice("SuiFederation")]
	public class SuiFederation : Microservice, IFederatedInventory<SuiIdentity>
	{
		private readonly Configuration _configuration;
		private readonly WalletService _walletService;
		private readonly MintingService _mintingService;
		private readonly TransactionManager _transactionManager;

		[ConfigureServices]
		public static void Configure(IServiceBuilder serviceBuilder)
		{
			serviceBuilder.Builder.RegisterServices();
		}
		
		[InitializeServices]
		public static async Task Initialize(IServiceInitializer initializer)
		{
			try
			{
				await initializer.GetService<WalletService>().InitializeObjects();
			}
			catch (Exception ex)
			{
				BeamableLogger.LogException(ex);
				BeamableLogger.LogWarning("Service initialization failed. Please fix the issues before using the service.");
			}
		}

		public SuiFederation(WalletService walletService, Configuration configuration, MintingService mintingService, TransactionManager transactionManager)
		{
			_walletService = walletService;
			_configuration = configuration;
			_mintingService = mintingService;
			_transactionManager = transactionManager;
		}

		public async Promise<FederatedAuthenticationResponse> Authenticate(string token, string challenge, string solution)
		{
			if (Context.UserId == 0) throw new UserRequiredException();
			if (string.IsNullOrEmpty(token))
			{
				//Generate user wallet
				var wallet = await _walletService.GetOrCreateWallet(Context.UserId.ToString());
				return new FederatedAuthenticationResponse
				{
					user_id = wallet.Address
				};
			}
			if (!string.IsNullOrEmpty(challenge) && !string.IsNullOrEmpty(solution))
			{
				if (await _walletService.VerifySignature(token, challenge, solution))
					// User identity is confirmed
					return new FederatedAuthenticationResponse
					{
						user_id = token
					};

				// Signature is invalid, user identity isn't confirmed
				BeamableLogger.LogWarning(
					"Invalid signature {signature} for challenge {challenge} and account {account}", solution,
					challenge, token);
				throw new UnauthorizedException();
			}

			return new FederatedAuthenticationResponse
			{
				challenge = $"Please sign this random message to authenticate: {Guid.NewGuid()}",
				challenge_ttl = await _configuration.AuthenticationChallengeTtlSec
			};
		}

		public async Promise<FederatedInventoryProxyState> StartInventoryTransaction(string id, string transaction, Dictionary<string, long> currencies, List<FederatedItemCreateRequest> newItems, List<FederatedItemDeleteRequest> deleteItems, List<FederatedItemUpdateRequest> updateItems)
		{
			return await _transactionManager.WithTransactionAsync(id, transaction, Context.UserId, async () =>
			{
				if (currencies.Any() || newItems.Any())
				{
					var currencyMints = currencies
						.Select(c => new MintRequest
						{
							ContentId = c.Key,
							Amount = (uint)c.Value,
							Properties = new Dictionary<string, string>()
						});

					var itemMints = newItems.Select(i => new MintRequest
					{
						ContentId = i.contentId,
						Amount = 1,
						Properties = i.properties
					});

					await _mintingService.Mint(Context.UserId, id, transaction, currencyMints.Union(itemMints).ToList());
				}
				return await GetInventoryState(id);
			});
		}

		public async Promise<FederatedInventoryProxyState> GetInventoryState(string id)
		{
			return await _walletService.GetInventoryState(id);
		}
	}
}