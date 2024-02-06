import { verifyPersonalMessage } from '@mysten/sui.js/verify';
import { getFullnodeUrl,GetOwnedObjectsParams, PaginatedObjectsResponse, SuiClient } from '@mysten/sui.js/client';
import { fromHEX, SUI_FRAMEWORK_ADDRESS, fromB64 } from '@mysten/sui.js/utils';
import { TransactionBlock } from '@mysten/sui.js/transactions';
import { Ed25519Keypair } from '@mysten/sui.js/keypairs/ed25519';
import {
    InventoryMintRequest,
    SuiBalance,
    SuiCapObject,
    SuiCapObjects, SuiCoinBalance,
    SuiKeys,
    SuiObject,
    SuiTransactionResult
} from './models';
import { retrievePaginatedData } from "./utils";
import { bech32 } from 'bech32';

type Callback<T> = (error: any, result: T | null) => void;
type Environment = 'mainnet' | 'testnet' | 'devnet' | 'localnet';
const SUI_PRIVATE_KEY_PREFIX = 'suiprivkey';

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
async function exportSecret(callback: Callback<string>) {
    let error = null;
    const keys= new SuiKeys()
    try {
        const keypair = new Ed25519Keypair();
        keys.Private = keypair.export().privateKey;
        keys.Public = keypair.toSuiAddress()
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
        const verificationPublicKey = await verifyPersonalMessage(messageEncoded, solution);
        if (verificationPublicKey.toSuiAddress() === token) {
            isValid = true;
        }
    } catch (ex) {
        error = ex;
    }
    callback(error, isValid);
}
async function getBalance(callback: Callback<string>, address: string, packageId: string, coinModules: string[], environment: Environment) {
    let error = null;
    let suiBalance = new SuiBalance();
    try {
        const suiClient = new SuiClient({url: getFullnodeUrl(environment)});
        for (const coin of coinModules) {
            const coinBalance = await suiClient.getBalance({
                owner: address,
                coinType: `${packageId}::${coin.toLowerCase()}::${coin.toUpperCase()}`
            });
            suiBalance.coins.push(new SuiCoinBalance(coin, Number.parseInt(coinBalance.totalBalance)));
        }
    } catch (ex) {
        error = ex;
    }
    callback(error, JSON.stringify(suiBalance));
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
async function mintInventory(callback: Callback<string>, packageId: string, token: string, items: string, secretKey: string, environment: Environment) {
    let error = null;
    const result = new SuiTransactionResult();
    try {
        const mintRequest: InventoryMintRequest = JSON.parse(items);
        const keypair = Ed25519Keypair.fromSecretKey(fromHEX(secretKey));
        const txb = new TransactionBlock();

        if (mintRequest.CurrencyItems != null) {
            mintRequest.CurrencyItems.forEach((coinItem) => {
                const coinTarget: `${string}::${string}::${string}` = `${packageId}::${coinItem.Name.toLowerCase()}::mint`;
                txb.moveCall({
                    target: coinTarget,
                    arguments: [
                        txb.object(coinItem.TreasuryCap),
                        txb.pure.u64(coinItem.Amount),
                        txb.pure.address(token)
                    ]});
            });
        }

        if (mintRequest.GameItems != null) {
            mintRequest.GameItems.forEach((gameItem) => {
                const itemTarget: `${string}::${string}::${string}` = `${packageId}::${gameItem.ContentName.toLowerCase()}::create`;

                txb.moveCall({
                    target: itemTarget,
                    arguments: [
                        txb.object(gameItem.GameAdminCap),
                        txb.pure.address(token),
                        txb.pure.string(gameItem.Name),
                        txb.pure.string(gameItem.Description),
                        txb.pure.string(gameItem.ImageURL),
                        txb.pure(gameItem.Attributes.map(attribute => attribute.Name)),
                        txb.pure(gameItem.Attributes.map(attribute => attribute.Value))
                    ]});
            });
        }

        const suiClient = new SuiClient({url: getFullnodeUrl(environment)});
        const response = await suiClient.signAndExecuteTransactionBlock({
            signer: keypair,
            transactionBlock: txb,
            options: {
                showEffects: true
            }
        });

        if (response.effects != null) {
            result.status = response.effects.status.status;
            result.computationCost = response.effects.gasUsed.computationCost;
            result.digest = response.effects.transactionDigest;
            result.objectIds = response.effects.created?.map(o => o.reference.objectId);
            result.error = response.effects.status.error;
        }
    } catch (ex) {
        error = ex;
    }
    callback(error, JSON.stringify(result));
}
async function getCapObjects(callback: Callback<string>, secretKey: string, packageId: string, environment: Environment) {
    let error = null;
    let results;
    const suiCaps: SuiCapObjects = new SuiCapObjects();
    try {
        const suiClient = new SuiClient({url: getFullnodeUrl(environment)});
        const keypair = Ed25519Keypair.fromSecretKey(decodeSuiPrivateKey(secretKey));
        const inputParams: GetOwnedObjectsParams = {
            owner: keypair.toSuiAddress(),
            cursor: null,
            options: {
                showType: true
            }
        };

        const handleData = async (input: GetOwnedObjectsParams) => {
            return await suiClient.getOwnedObjects(input);
        };

        results = await retrievePaginatedData<GetOwnedObjectsParams, PaginatedObjectsResponse>(handleData, inputParams);

        results.forEach(result => {
            result.data.forEach(element => {
                if (element.data != undefined)  {
                    if (element.data.type != undefined) {
                        if (element.data.type.startsWith(packageId) && element.data.type.endsWith("GameAdminCap"))  {
                            //This is GameAdminCap object
                            const parts = element.data.type.split("::");
                            suiCaps.GameAdminCaps.push(new SuiCapObject(element.data.objectId, parts[1]))
                        }
                        if (element.data.type.startsWith(`${SUI_FRAMEWORK_ADDRESS}::coin::TreasuryCap<${packageId}`))  {
                            //This is TreasuryCap object
                            const parts = element.data.type.split("::");
                            suiCaps.TreasuryCaps.push(new SuiCapObject(element.data.objectId, parts[3]))
                        }
                    }
                }
            });
        })
    } catch (ex) {
        error = ex;
    }
    callback(error, JSON.stringify(suiCaps));
}

module.exports = {
    exportSecret,
    verifySignature,
    getBalance,
    getOwnedObjects,
    getCapObjects,
    mintInventory
};