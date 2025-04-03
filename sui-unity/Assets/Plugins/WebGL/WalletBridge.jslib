mergeInto(LibraryManager.library, {
    ConnectWallet: function () {
        window.bridge.connectWallet("Stashed")
            .then(addr => {
                SendMessage("UiWalletManager", "OnWalletConnected", addr);
            })
            .catch(() => {
                SendMessage("UiWalletManager", "OnWalletConnected", "");
            });
    },

    DisconnectWallet: function () {
        window.bridge.disconnectWallet("Stashed")
            .then(() => {
                SendMessage("UiWalletManager", "OnWalletDisconnected", "");
            })
            .catch(() => {
                SendMessage("UiWalletManager", "OnWalletDisconnected", "");
            });
    },

    SignMessage: function (msgPtr) {
        const msg = UTF8ToString(msgPtr);
        window.bridge.signMessage("Stashed", msg)
            .then(signed => {
                SendMessage("UiWalletManager", "OnMessageSigned", signed);
            })
            .catch(() => {
                SendMessage("UiWalletManager", "OnMessageSigned", "");
            });
    }
});
