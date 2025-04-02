var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
import { getWallets, StandardConnect, StandardDisconnect } from "@mysten/wallet-standard";
import { registerStashedWallet } from "@mysten/zksend";
registerStashedWallet("Stashed");
let cachedWallets = [];
let connectedAccount;
function loadInstalledWallets() {
    return __awaiter(this, void 0, void 0, function* () {
        return Array.from(getWallets().get());
    });
}
function getStoredWallets() {
    return __awaiter(this, void 0, void 0, function* () {
        if (cachedWallets.length === 0) {
            cachedWallets = yield loadInstalledWallets();
        }
        return cachedWallets;
    });
}
function getSelectedWallet(name) {
    return __awaiter(this, void 0, void 0, function* () {
        const wallets = yield getStoredWallets();
        const wallet = wallets.find(w => w.name === name);
        return wallet;
    });
}
function getConnectedAccount(name) {
    return __awaiter(this, void 0, void 0, function* () {
        if (connectedAccount == undefined) {
            connectedAccount = yield connectAccount(name);
        }
        return connectedAccount;
    });
}
function connectAccount(name) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const wallet = yield getSelectedWallet(name);
            const connectFeature = wallet.features[StandardConnect];
            const result = yield connectFeature.connect();
            connectedAccount = result.accounts[0];
            return connectedAccount;
        }
        catch (error) {
            console.error("Connection failed:", error);
        }
    });
}
export function loadWallets() {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            return (yield getStoredWallets()).map(w => w.name);
        }
        catch (error) {
            console.error("Error loading wallets:", error);
            return [];
        }
    });
}
export function connectWallet(name) {
    return __awaiter(this, void 0, void 0, function* () {
        var _a;
        try {
            const account = yield getConnectedAccount(name);
            return (_a = account === null || account === void 0 ? void 0 : account.address) !== null && _a !== void 0 ? _a : "";
        }
        catch (error) {
            console.error("Connection failed:", error);
            return "";
        }
    });
}
export function disconnectWallet(name) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const wallet = yield getSelectedWallet(name);
            if (wallet == undefined) {
                return;
            }
            const feature = wallet.features[StandardDisconnect];
            yield feature.disconnect();
            connectedAccount = undefined;
            cachedWallets = [];
        }
        catch (error) {
            console.error("Disconnect failed:", error);
            return;
        }
    });
}
export function signMessage(name, message) {
    return __awaiter(this, void 0, void 0, function* () {
        if (!message) {
            return "";
        }
        const wallet = yield getSelectedWallet(name);
        if (wallet == undefined) {
            return "";
        }
        try {
            const connectFeature = wallet.features[StandardConnect];
            const connectOutput = yield connectFeature.connect();
            const signMessageFeature = wallet.features["sui:signPersonalMessage"];
            const signedMessage = yield signMessageFeature.signPersonalMessage({
                message: new TextEncoder().encode(message),
                account: connectOutput.accounts[0],
            });
            return signedMessage.signature;
        }
        catch (error) {
            console.error("Signing failed:", error);
            return "";
        }
    });

    window.ConnectWallet = async function () {
        const walletSelect = document.getElementById("wallets");
        const walletName = walletSelect.value;
        const address = await connectWallet(walletName);
        unityInstance.SendMessage("WalletManager", "OnWalletConnected", address);
    };

    window.DisconnectWallet = async function () {
        const walletSelect = document.getElementById("wallets");
        const walletName = walletSelect.value;
        await disconnectWallet(walletName);
        unityInstance.SendMessage("WalletManager", "OnWalletDisconnected");
    };

    window.SignMessage = async function (message) {
        const walletSelect = document.getElementById("wallets");
        const walletName = walletSelect.value;
        const signed = await signMessage(walletName, message);
        unityInstance.SendMessage("WalletManager", "OnMessageSigned", signed);
    };
}
