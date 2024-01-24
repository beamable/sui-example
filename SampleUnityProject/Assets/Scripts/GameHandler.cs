using System.Linq;
using Beamable;
using Beamable.Common.Api.Inventory;
using Beamable.Server.Clients;
using SuiFederationCommon;
using SuiFederationCommon.Content;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    private BeamContext Context => BeamContext.Default;
    private SuiFederationClient _suiFederationClient;

    [SerializeField] private TextMeshProUGUI uiPlayerId;
    [SerializeField] private TextMeshProUGUI uiWallet;
    [SerializeField] private Button btnInitWallet;
    [SerializeField] private Button btnOpenWallet;
    [SerializeField] private Button btnOpenInventory;
    [SerializeField] private TextMeshProUGUI uiInventoryCoins;
    [SerializeField] private TextMeshProUGUI uiInventoryNft;
    [SerializeField] private GameObject inventorySection;
    async void Start()
    {
        inventorySection.SetActive(false);

        uiPlayerId.text = "initializing...";
        btnInitWallet.gameObject.SetActive(false);
        btnOpenWallet.gameObject.SetActive(false);

        btnInitWallet.onClick.AddListener(InitWallet);
        btnOpenWallet.onClick.AddListener(OpenWallet);
        btnOpenInventory.onClick.AddListener(OpenInventory);

        await Context.OnReady;
        await Context.Accounts.OnReady;
        uiPlayerId.text = Context.PlayerId.ToString();

        btnInitWallet.gameObject.SetActive(true);

        ShowWalletInfo();

        Context.Api.InventoryService.Subscribe(SyncInventory);
    }

    private async void InitWallet()
    {
        if (!Context.Accounts.Current.ExternalIdentities.Any())
        {
            uiWallet.text = "initializing...";
            await Context.Accounts.AddExternalIdentity<SuiIdentity, SuiFederationClient>("");
        }
        else
        {
            Debug.Log("Wallet already initialized");
        }

        ShowWalletInfo();
    }

    private void ShowWalletInfo()
    {
        if (Context.Accounts.Current.ExternalIdentities.Any())
        {
            uiWallet.text = Context.Accounts.Current.ExternalIdentities.First().userId;
            btnOpenWallet.gameObject.SetActive(true);
            inventorySection.SetActive(true);
        }
    }

    private async void OpenWallet()
    {
        if (uiWallet.text.StartsWith("0x"))
        {
            _suiFederationClient = Context.Microservices().SuiFederation();
            var networkEnvironment = await _suiFederationClient.GetSuiEnvironment();
            Application.OpenURL($"https://suiexplorer.com/address/{uiWallet.text}?deviceId={PlayerSettings.applicationIdentifier}&network={networkEnvironment}");
        }
    }

    private void OpenInventory()
    {
        Application.OpenURL($"https://portal.beamable.com/{Context.Cid}/games/{Context.Pid}/realms/{Context.Pid}/players/{Context.PlayerId}/inventory");
    }

    private async void SyncInventory(InventoryView inventory)
    {
        var coins = 0L;
        var nftCount = 0;

        foreach (var currency in inventory.currencies)
        {
            var contentObject = await Context.Content.GetContent(currency.Key);
            if (contentObject is BlockchainCurrency)
                coins += currency.Value;
        }

        foreach (var item in inventory.items)
        {
            var contentObject = await Context.Content.GetContent(item.Key);
            if (contentObject is BlockchainItem)
                nftCount += item.Value.Count;
        }

        uiInventoryCoins.text = coins.ToString();
        uiInventoryNft.text = nftCount.ToString();
    }
}