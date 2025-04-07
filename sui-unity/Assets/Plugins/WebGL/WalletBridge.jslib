mergeInto(LibraryManager.library, {

    PromptForPopupAndConnect: function () {
        const message = "🔐 To connect your wallet, please allow popups in your browser.\n\n" +
            "Wallet signing may fail if popups are blocked.\n\n" +
            "Would you like to attempt connection now?";

        const proceed = window.confirm(message);

        if (proceed) {
            // Attempt a dummy popup to trigger browser's "unblock popup" warning if needed
            const popupTest = window.open("about:blank", "_blank", "width=1,height=1");

            if (!popupTest || popupTest.closed || typeof popupTest.closed === 'undefined') {
                alert("⚠️ Popup was blocked. Please allow popups for this site and try again.");
            } else {
                popupTest.close(); // We're done, we just needed to trigger the attempt
            }
        }
    },

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

        // Sign directly
        window.bridge.signMessage("Stashed", msg)
            .then((signed) => {
                if (typeof SendMessage === "function") {
                    SendMessage("UiWalletManager", "OnMessageSigned", signed);
                } else {
                    console.error("SendMessage is not defined");
                }
            })
            .catch((err) => {
                console.error("Signing failed:", err);
                if (typeof SendMessage === "function") {
                    SendMessage("UiWalletManager", "OnMessageSigned", "");
                }
            });
    }
    
});
