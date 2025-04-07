
mergeInto(LibraryManager.library, {
    ConnectWallet: function(callbackPtr) {
        WalletBridge.connectWallet("Stashed")
            .then(addr => sendStringToUnity(callbackPtr, addr))
            .catch(() => sendStringToUnity(callbackPtr, ""));
    },
    DisconnectWallet: function(callbackPtr) {
        WalletBridge.disconnectWallet("Stashed")
            .then(() => sendStringToUnity(callbackPtr, ""))
            .catch(() => sendStringToUnity(callbackPtr, ""));
    },
    SignMessage: function(msgPtr, callbackPtr) {
        const msg = UTF8ToString(msgPtr);
        WalletBridge.signMessage("Stashed", msg)
            .then(signed => sendStringToUnity(callbackPtr, signed))
            .catch(() => sendStringToUnity(callbackPtr, ""));
    }
});

function sendStringToUnity(callbackPtr, str) {
    const buffer = allocate(intArrayFromString(str), 'i8', ALLOC_STACK);
    dynCall('vii', callbackPtr, [buffer]);
}
