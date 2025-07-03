mergeInto(LibraryManager.library, {
    PromptForPopupAndConnect: function() {
        var proceed = window.confirm(
            "🔐 To connect your wallet, please allow popups in your browser.\n\n" +
            "Wallet signing may fail if popups are blocked.\n\n" +
            "Would you like to attempt connection now?"
        );
        if (typeof SendMessage === "function") {
            try { SendMessage("UiWalletManager", "OnPopupResult", proceed ? "1" : "0"); }
            catch(e){ console.warn("SendMessage failed:", e); }
        }
        if (proceed) {
            var popupTest = window.open("about:blank","_blank","width=1,height=1");
            if (!popupTest || popupTest.closed || typeof popupTest.closed==='undefined') {
                alert("⚠️ Popup was blocked. Please allow popups for this site and try again.");
            } else {
                popupTest.close();
            }
        }
    },
    
    ConnectWallet: function () {
        window.WalletBridge
            .connectWallet("Slush")
            .then(function(addr) {
                if (typeof SendMessage === "function") {
                    try { SendMessage("UiWalletManager", "OnWalletConnected", addr||""); }
                    catch(e){ console.warn("SendMessage failed:", e); }
                }
            })
            .catch(function() {
                if (typeof SendMessage === "function") {
                    try { SendMessage("UiWalletManager", "OnWalletConnected", ""); }
                    catch(e){ console.warn("SendMessage failed:", e); }
                }
            });
    },

    DisconnectWallet: function () {
        window.WalletBridge
            .disconnectWallet("Slush")
            .then(function() {
                if (typeof SendMessage === "function") {
                    try { SendMessage("UiWalletManager", "OnWalletDisconnected", ""); }
                    catch(e){ console.warn("SendMessage failed:", e); }
                }
            })
            .catch(function() {
                if (typeof SendMessage === "function") {
                    try { SendMessage("UiWalletManager", "OnWalletDisconnected", ""); }
                    catch(e){ console.warn("SendMessage failed:", e); }
                }
            });
    },

    SignMessage: function(msgPtr) {
        var msg = UTF8ToString(msgPtr);
        window.WalletBridge
            .signMessage("Slush", msg)
            .then(function(sig) {
                if (typeof SendMessage === "function") {
                    try { SendMessage("UiWalletManager", "OnMessageSigned", sig||""); }
                    catch(e){ console.warn("SendMessage failed:", e); }
                }
            })
            .catch(function(err) {
                console.error("Signing failed:", err);
                if (typeof SendMessage === "function") {
                    try { SendMessage("UiWalletManager", "OnMessageSigned", ""); }
                    catch(e){ console.warn("SendMessage failed:", e); }
                }
            });
    },

    LoadWallets: function() {
        window.WalletBridge
            .loadWallets()
            .then(function(list) {
                if (typeof SendMessage === "function") {
                    try { SendMessage("UiWalletManager", "OnWalletsLoaded", JSON.stringify(list)); }
                    catch(e){ console.warn("SendMessage failed:", e); }
                }
            })
            .catch(function() {
                if (typeof SendMessage === "function") {
                    try { SendMessage("UiWalletManager", "OnWalletsLoaded", "[]"); }
                    catch(e){ console.warn("SendMessage failed:", e); }
                }
            });
    }
});
