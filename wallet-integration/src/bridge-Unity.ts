import { getWallets, Wallet, WalletAccount, StandardConnect, StandardDisconnect, StandardDisconnectFeature, StandardConnectFeature, SuiSignPersonalMessageFeature  } from "@mysten/wallet-standard";
import { registerSlushWallet } from '@mysten/slush-wallet';

registerSlushWallet("Slush");

let cachedWallets: Wallet[] = [];
let connectedAccount: WalletAccount | undefined;

async function loadInstalledWallets(): Promise<Wallet[]> {
    return Array.from(getWallets().get());
}

async function getStoredWallets(): Promise<Wallet[]> {
    if (cachedWallets.length === 0) {
        cachedWallets = await loadInstalledWallets();
    }
    return cachedWallets;
}

async function getSelectedWallet(name: string): Promise<Wallet | undefined> {
    const wallets = await getStoredWallets();
    const wallet = wallets.find(w => w.name === name);
    return wallet;
}

async function getConnectedAccount(name: string): Promise<WalletAccount | undefined> {
    if (connectedAccount == undefined) {
        connectedAccount = await connectAccount(name);
    }
    return connectedAccount;
}

async function connectAccount(name: string): Promise<WalletAccount | undefined> {
    try {
        const wallet = await getSelectedWallet(name);
        const connectFeature = wallet!.features[StandardConnect] as StandardConnectFeature[typeof StandardConnect];
        const result = await connectFeature.connect();
        connectedAccount = result.accounts[0];
        return connectedAccount;

    } catch (error) {
        console.error("Connection failed:", error);
    }
}

async function loadWallets(): Promise<string[]> {
    try {
        return (await getStoredWallets()).map(w => w.name);
    } catch (error) {
        console.error("Error loading wallets:", error);
        return [];
    }
}

async function connectWallet(name: string): Promise<string> {
    try {
        const account = await getConnectedAccount(name);
        return account?.address ?? "";
    } catch (error) {
        console.error("Connection failed:", error);
        return "";
    }
}

async function disconnectWallet(name: string): Promise<void> {
    try {
        const wallet = await getSelectedWallet(name);
        if (wallet == undefined) {
            return;
        }
        const feature = wallet.features[StandardDisconnect] as StandardDisconnectFeature[typeof StandardDisconnect];
        await feature.disconnect();
        connectedAccount = undefined;
        cachedWallets = [];
    } catch (error) {
        console.error("Disconnect failed:", error);
        return;
    }
}

async function signMessage(name: string, message: string): Promise<string> {
    if (!message) {
        return "";
    }

    const wallet = await getSelectedWallet(name);

    if (wallet == undefined) {
        return "";
    }

    try {
        const connectFeature = wallet.features[StandardConnect] as StandardConnectFeature[typeof StandardConnect];
        const connectOutput = await connectFeature.connect();
        const signMessageFeature = wallet.features["sui:signPersonalMessage"] as SuiSignPersonalMessageFeature["sui:signPersonalMessage"];
        const signedMessage = await signMessageFeature.signPersonalMessage({
            message: new TextEncoder().encode(message),
            account: connectOutput.accounts[0],
        });
        return signedMessage.signature;

    } catch (error) {
        console.error("Signing failed:", error);
        return "";
    }
}

export {
    loadWallets,
    connectWallet,
    disconnectWallet,
    signMessage
};

(window as any).bridge = {
    connectWallet,
    disconnectWallet,
    signMessage,
    loadWallets
};