using HDWallet.Core;
using HDWallet.Ed25519;

namespace SuiFederationService.Features.Accounts;

public class SuiWallet : Wallet
{
    protected override IAddressGenerator GetAddressGenerator() => SuiAddressGenerator.AccountAddressGenerator;

    public SuiWallet()
    {
    }

    public SuiWallet(string privateKeyHex) : base(privateKeyHex)
    {
    }

    public SuiWallet(byte[] privateKey) : base(privateKey)
    {
    }
}