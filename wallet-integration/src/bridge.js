import { getWallets, StandardConnect, StandardDisconnect } from "@mysten/wallet-standard";
import { registerStashedWallet } from "@mysten/zksend";
registerStashedWallet("Stashed");
let cachedWallets = [];
let connectedAccount;
async function loadInstalledWallets() {
    return Array.from(getWallets().get());
}
async function getStoredWallets() {
    if (cachedWallets.length === 0) {
        cachedWallets = await loadInstalledWallets();
    }
    return cachedWallets;
}
async function getSelectedWallet(name) {
    const wallets = await getStoredWallets();
    const wallet = wallets.find(w => w.name === name);
    return wallet;
}
async function getConnectedAccount(name) {
    if (connectedAccount == undefined) {
        connectedAccount = await connectAccount(name);
    }
    return connectedAccount;
}
async function connectAccount(name) {
    try {
        const wallet = await getSelectedWallet(name);
        const connectFeature = wallet.features[StandardConnect];
        const result = await connectFeature.connect();
        connectedAccount = result.accounts[0];
        return connectedAccount;
    }
    catch (error) {
        console.error("Connection failed:", error);
    }
}
export async function loadWallets() {
    try {
        return (await getStoredWallets()).map(w => w.name);
    }
    catch (error) {
        console.error("Error loading wallets:", error);
        return [];
    }
}
export async function connectWallet(name) {
    try {
        const account = await getConnectedAccount(name);
        return account?.address ?? "";
    }
    catch (error) {
        console.error("Connection failed:", error);
        return "";
    }
}
export async function disconnectWallet(name) {
    try {
        const wallet = await getSelectedWallet(name);
        if (wallet == undefined) {
            return;
        }
        const feature = wallet.features[StandardDisconnect];
        await feature.disconnect();
        connectedAccount = undefined;
        cachedWallets = [];
    }
    catch (error) {
        console.error("Disconnect failed:", error);
        return;
    }
}
export async function signMessage(name, message) {
    if (!message) {
        return "";
    }
    const wallet = await getSelectedWallet(name);
    if (wallet == undefined) {
        return "";
    }
    try {
        const connectFeature = wallet.features[StandardConnect];
        const connectOutput = await connectFeature.connect();
        const signMessageFeature = wallet.features["sui:signPersonalMessage"];
        const signedMessage = await signMessageFeature.signPersonalMessage({
            message: new TextEncoder().encode(message),
            account: connectOutput.accounts[0],
        });
        return signedMessage.signature;
    }
    catch (error) {
        console.error("Signing failed:", error);
        return "";
    }
}
