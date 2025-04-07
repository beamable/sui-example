declare function loadWallets(): Promise<string[]>;
declare function connectWallet(name: string): Promise<string>;
declare function disconnectWallet(name: string): Promise<void>;
declare function signMessage(name: string, message: string): Promise<string>;
export { loadWallets, connectWallet, disconnectWallet, signMessage };
