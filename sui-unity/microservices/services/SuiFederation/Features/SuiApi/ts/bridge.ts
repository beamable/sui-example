import {GasCostSummary, getFullnodeUrl, SuiClient, GetOwnedObjectsParams, PaginatedObjectsResponse, SuiObjectRef} from '@mysten/sui/client';
import { verifyPersonalMessageSignature } from '@mysten/sui/verify';
import { fromHEX, fromB64 } from '@mysten/sui/utils';
import { Ed25519Keypair } from '@mysten/sui/keypairs/ed25519';
import { bech32 } from 'bech32';
import { Transaction } from '@mysten/sui/transactions';
import {
    CreateWalletResponse,
    NftMintMessage,
    RegularCoinMintMessage,
    SuiTransactionResult,
    CoinBalanceResponse,
    CoinBalanceRequest,
    SuiObject,
    RegularCoinBurnMessage
} from './models';
import { retrievePaginatedData } from "./utils";

type Callback<T> = (error: any, result: T | null) => void;
type Environment = 'mainnet' | 'testnet' | 'devnet' | 'localnet';
const SUI_PRIVATE_KEY_PREFIX = 'suiprivkey';
let suiClientInstance: SuiClient | null = null;

function getSuiClientInstance(environment: Environment): SuiClient {
    if (!suiClientInstance) {
        suiClientInstance = new SuiClient({url: getFullnodeUrl(environment)});
    }
    return suiClientInstance;
}

function decodeSuiPrivateKey(value: string) {
    if (value.startsWith("0x")) {
        return fromHEX(value);
    }
    if (value.startsWith(SUI_PRIVATE_KEY_PREFIX)) {
        const { prefix, words } = bech32.decode(value);
        const extendedSecretKey = new Uint8Array(bech32.fromWords(words));
        return extendedSecretKey.slice(1);
    }
    if (!value.startsWith(SUI_PRIVATE_KEY_PREFIX) && value.length == 60) {
        const pk_with_prefix = SUI_PRIVATE_KEY_PREFIX + value;
        const { prefix, words } = bech32.decode(pk_with_prefix);
        const extendedSecretKey = new Uint8Array(bech32.fromWords(words));
        return extendedSecretKey.slice(1);
    }
    if (!value.startsWith(SUI_PRIVATE_KEY_PREFIX) && value.length == 44) {
        return fromB64(value);
    }
    throw new Error('invalid private key value');
}

async function createWallet(callback: Callback<string>) {
    let error = null;
    const keys= new CreateWalletResponse();
    try {
        const keypair = new Ed25519Keypair();
        keys.PrivateKey = keypair.getSecretKey();
        keys.PublicKey = keypair.toSuiAddress();
    } catch (ex) {
        error = ex;
    }
    callback(error, JSON.stringify(keys));
}

async function importWallet(callback: Callback<string>, privateKey: string) {
    let error = null;
    const keys= new CreateWalletResponse();
    try {
        const decoded = decodeSuiPrivateKey(privateKey);
        const keypair = Ed25519Keypair.fromSecretKey(decoded);
        keys.PrivateKey = keypair.getSecretKey();
        keys.PublicKey = keypair.toSuiAddress();
    } catch (ex) {
        error = ex;
    }
    callback(error, JSON.stringify(keys));
}

async function verifySignature(callback: Callback<boolean>, token: string, challenge: string, solution: string) {
    let isValid = false;
    let error = null;
    try {
        const messageEncoded = new TextEncoder().encode(challenge);
        const verificationPublicKey = await verifyPersonalMessageSignature(messageEncoded, solution);
        if (verificationPublicKey.toSuiAddress() === token) {
            isValid = true;
        }
    } catch (ex) {
        if (ex instanceof Error && ex.message === `Signature is not valid for the provided message`) {
            isValid = false;
        } else if (ex instanceof Error && ex.message.startsWith('Unsupported signature scheme')) {
            isValid = false;
        } else {
            isValid = false;
            error = ex;
        }
    }
    callback(error, isValid);
}

async function mintRegularCoin(callback: Callback<string>, item: string, realmKey: string, environment: Environment) {
    let error = null;
    const result = new SuiTransactionResult();
    try {
        const mintRequests: RegularCoinMintMessage[] = JSON.parse(item);
        const keypair = Ed25519Keypair.fromSecretKey(realmKey);
        const txb = new Transaction();

        mintRequests.forEach((coinItem) => {
            const coinTarget: `${string}::${string}::${string}` = `${coinItem.PackageId}::${coinItem.Module}::${coinItem.Function}`;
            txb.moveCall({
                target: coinTarget,
                arguments: [
                    txb.object(coinItem.AdminCap),
                    txb.object(coinItem.TreasuryCap),
                    txb.pure.u64(coinItem.Amount),
                    txb.pure.address(coinItem.PlayerWalletAddress)
                ]});
        });

        const suiClient = getSuiClientInstance(environment);
        const response = await suiClient.signAndExecuteTransaction({
            signer: keypair,
            transaction: txb,
            options: {
                showEffects: true
            }
        });

        if (response.effects != null) {
            result.status = response.effects.status.status;
            result.gasUsed = calculateTotalCost(response.effects.gasUsed);
            result.digest = response.effects.transactionDigest;
            result.objectIds = response.effects.created?.map(o => o.reference.objectId);
            result.error = response.effects.status.error;
        }

    } catch (ex) {
        error = ex;
    }
    callback(error, JSON.stringify(result));
}

async function getBalance(callback: Callback<string>, address: string, item: string, environment: Environment) {
    let error = null;
    let coinBalanceResponse = new CoinBalanceResponse("",0);
    try {
        const suiClient = getSuiClientInstance(environment);
        const request: CoinBalanceRequest = JSON.parse(item);
        const coinType = `${request.PackageId}::${request.ModuleName.toLowerCase()}::${request.ModuleName.toUpperCase()}`;
        const coinBalance = await suiClient.getBalance({
            owner: address,
            coinType: coinType
        });
        coinBalanceResponse.CoinType = coinType;
        coinBalanceResponse.Total = parseInt(coinBalance.totalBalance ?? "0", 10) || 0;

    } catch (ex) {
        error = ex;
    }
    callback(error, JSON.stringify(coinBalanceResponse));
}

async function mintNfts(callback: Callback<string>, item: string, realmKey: string, environment: Environment) {
    let error = null;
    const result = new SuiTransactionResult();
    try {
        const mintRequests: NftMintMessage[] = JSON.parse(item);
        const keypair = Ed25519Keypair.fromSecretKey(realmKey);
        const txb = new Transaction();

        mintRequests.forEach((item) => {
            const target: `${string}::${string}::${string}` = `${item.PackageId}::${item.Module}::${item.Function}`;
            txb.moveCall({
                target: target,
                arguments: [
                    txb.object(item.AdminCap),
                    txb.pure.address(item.PlayerWalletAddress),
                    txb.pure.string(item.NftContentItem.Name),
                    txb.pure.string(item.NftContentItem.Description),
                    txb.pure.string(item.NftContentItem.Url),
                    txb.pure.string(item.NftContentItem.ContentId),
                    txb.pure.vector("string", item.NftContentItem.Attributes.map(attribute => attribute.Name)),
                    txb.pure.vector("string", item.NftContentItem.Attributes.map(attribute => attribute.Value))


                ]});
        });

        const suiClient = getSuiClientInstance(environment);
        const response = await suiClient.signAndExecuteTransaction({
            signer: keypair,
            transaction: txb,
            options: {
                showEffects: true
            }
        });

        if (response.effects != null) {
            result.status = response.effects.status.status;
            result.gasUsed = calculateTotalCost(response.effects.gasUsed);
            result.digest = response.effects.transactionDigest;
            result.objectIds = response.effects.created?.map(o => o.reference.objectId);
            result.error = response.effects.status.error;
        }

    } catch (ex) {
        error = ex;
    }
    callback(error, JSON.stringify(result));
}

async function getOwnedObjects(callback: Callback<string>, address: string, packageId: string, environment: Environment) {
    let error = null;
    const objects: SuiObject[] = [];
    try {
        const suiClient = new SuiClient({url: getFullnodeUrl(environment)});

        const inputParams: GetOwnedObjectsParams = {
            owner: address,
            cursor: null,
            options: {
                showType: true,
                showContent: true,
                showDisplay: true
            },
            filter: {
                Package: packageId
            }
        };
        const handleData = async (input: GetOwnedObjectsParams) => {
            return await suiClient.getOwnedObjects(input);
        };

        const results = await retrievePaginatedData<GetOwnedObjectsParams, PaginatedObjectsResponse>(handleData, inputParams);

        results.forEach(result => {
            result.data.forEach(element => {
                if (element.data != undefined)  {
                    const suiObject = new SuiObject(
                        element.data.objectId,
                        element.data.digest,
                        element.data.version,
                        element.data.content,
                        element.data.display
                    );
                    objects.push(suiObject);
                }
            });
        })

    } catch (ex) {
        error = ex;
    }

    callback(error, JSON.stringify(objects.map(object => object.toView())));
}

async function burnCoins(callback: Callback<string>, request: string, realmKey: string, environment: Environment) {
    let error = null;
    const burnRequests: RegularCoinBurnMessage[] = JSON.parse(request);
    let result = new SuiTransactionResult();
    try {
        const suiClient = getSuiClientInstance(environment);
        const gameKeypair = Ed25519Keypair.fromSecretKey(realmKey);
        const playerKeypair = Ed25519Keypair.fromSecretKey(burnRequests[0].PlayerWalletKey);
        const playerWallet = burnRequests[0].PlayerWalletAddress;
        const gameWallet = gameKeypair.toSuiAddress();
        const txb = new Transaction();

        for (const coinItem of burnRequests) {
            const coins = await suiClient.getCoins({
                owner: coinItem.PlayerWalletAddress,
                coinType: `${coinItem.PackageId}::${coinItem.Module.toLowerCase()}::${coinItem.Module.toUpperCase()}`
            });

            // Sort coins by balance (descending)
            const sortedCoins = coins.data.sort((a, b) => Number(b.balance) - Number(a.balance));

            // Select coins whose total balance matches the amount to burn
            let totalBalance = 0;
            const selectedCoins = [];
            for (const coin of sortedCoins) {
                if (totalBalance >= coinItem.Amount) break;
                selectedCoins.push(coin);
                totalBalance += Number(coin.balance);
            }

            let remainingAmount = coinItem.Amount;
            const coinsToBurn = [];
            for (const coin of selectedCoins) {
                const coinBalance = Number(coin.balance);
                if (coinBalance <= remainingAmount) {
                    // Use the entire coin
                    coinsToBurn.push(coin.coinObjectId);
                    remainingAmount -= coinBalance;
                } else {
                    // Split the coin to get the exact amount needed
                    const splitCoin = txb.splitCoins(txb.object(coin.coinObjectId), [txb.pure.u64(remainingAmount)]);
                    coinsToBurn.push(splitCoin);
                    remainingAmount = 0;
                }
                if (remainingAmount === 0) break;
            }
            const coinTarget: `${string}::${string}::${string}` = `${coinItem.PackageId}::${coinItem.Module}::${coinItem.Function}`;
            for (const coinId of coinsToBurn) {
                txb.moveCall({
                    target: coinTarget,
                    arguments: [
                        txb.object(coinItem.TreasuryCap),
                        txb.object(coinId),
                    ],
                });
            }
        }

        let payment: SuiObjectRef[] = [];
        const coins = await suiClient.getCoins({ owner: gameWallet, limit: 1 });
        if (coins.data.length > 0) {
            payment = coins.data.map((coin) => ({
                objectId: coin.coinObjectId,
                version: coin.version,
                digest: coin.digest,
            }));
        } else {
            throw new Error(`Can't find gas coins from sponsor ${gameWallet}.`);
        }

        const kindBytes = await txb.build({ onlyTransactionKind: true, client: suiClient });
        const sponsoredTxb = Transaction.fromKind(kindBytes);
        sponsoredTxb.setSender(playerWallet);
        sponsoredTxb.setGasOwner(gameWallet);
        sponsoredTxb.setGasPayment(payment);
        const sponsoredBytes = await sponsoredTxb.build({ client: suiClient });
        const developerSignature = await gameKeypair!.signTransaction(sponsoredBytes);
        const playerSignature = await playerKeypair!.signTransaction(sponsoredBytes);

        const response = await suiClient.executeTransactionBlock({
            transactionBlock: sponsoredBytes,
            signature: [developerSignature.signature, playerSignature.signature],
            options: {
                showEffects: true,
                showEvents: true,
                showObjectChanges: true,
            },
        });

        if (response.effects != null) {
            result.status = response.effects.status.status;
            result.gasUsed = calculateTotalCost(response.effects.gasUsed);
            result.digest = response.effects.transactionDigest;
            result.objectIds = response.effects.created?.map(o => o.reference.objectId);
            result.error = response.effects.status.error;
        }

    } catch (ex) {
        error = ex;
    }
    callback(error, JSON.stringify(result));
}

function calculateTotalCost(gasSummary: GasCostSummary): string {
    const computationCost = parseFloat(gasSummary.computationCost) || 0;
    const storageCost = parseFloat(gasSummary.storageCost) || 0;
    const storageRebate = parseFloat(gasSummary.storageRebate) || 0;
    const totalCost = computationCost + storageCost - storageRebate;
    return totalCost.toString();
}


module.exports = {
    createWallet,
    importWallet,
    verifySignature,
    mintRegularCoin,
    getBalance,
    mintNfts,
    getOwnedObjects,
    burnCoins
};