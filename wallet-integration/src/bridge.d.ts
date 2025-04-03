export declare function openWallet(name: string): Promise<void>;
export declare function loadWallets(): Promise<string[]>;
export declare function connectWallet(name: string): Promise<string>;
export declare function disconnectWallet(name: string): Promise<void>;
export declare function signMessage(name: string, message: string): Promise<string>;
