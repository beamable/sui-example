import { verifyPersonalMessage } from '@mysten/sui.js/verify';
import { getFullnodeUrl,GetOwnedObjectsParams, PaginatedObjectsResponse, SuiClient } from '@mysten/sui.js/client';
import { fromHEX, toHEX, SUI_FRAMEWORK_ADDRESS } from '@mysten/sui.js/utils';
import { TransactionBlock } from '@mysten/sui.js/transactions';
import { Ed25519Keypair } from '@mysten/sui.js/keypairs/ed25519';
import {InventoryMintRequest, SuiBalance, SuiCapObject, SuiKeys, SuiObject, SuiTransactionResult} from './models';
import { retrievePaginatedData } from "./utils";

type Callback<T> = (error: any, result: T | null) => void;
type Environment = 'mainnet' | 'testnet' | 'devnet' | 'localnet';
const MIN_GAS_BUDGET = 10000000;

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
async function getBalance(callback: Callback<string>, address: string, packageId: string, coinModule: string, environment: Environment) {
    let error = null;
    let suiBalance = new SuiBalance(coinModule, 0);
    try {
        const suiClient = new SuiClient({url: getFullnodeUrl(environment)});
        const coinBalance = await suiClient.getBalance({
            owner: address,
            coinType: `${packageId}::${coinModule.toLowerCase()}::${coinModule.toUpperCase()}`
        });
        suiBalance = new SuiBalance(coinBalance.coinType, Number.parseInt(coinBalance.totalBalance))
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
async function getCapObjects(callback: Callback<string>, secretKey: string, packageId: string, itemModule: string, coinModule: string, environment: Environment) {
    let error = null;
    let capObject = new SuiCapObject("","");
    try {
        const gameAdminCapType = "GameAdminCap";
        const treasuryCapType = "TreasuryCap";
        const suiClient = new SuiClient({url: getFullnodeUrl(environment)});
        const keypair = Ed25519Keypair.fromSecretKey(fromHEX(secretKey));
        const inputParams: GetOwnedObjectsParams = {
            owner: keypair.toSuiAddress(),
            cursor: null,
            options: {
                showType: true
            },
            filter: {
                MatchAny: [
                    {
                        StructType: `${packageId}::${itemModule.toLowerCase()}::${gameAdminCapType}`
                    },
                    {
                        StructType: `${SUI_FRAMEWORK_ADDRESS}::coin::${treasuryCapType}<${packageId}::${coinModule.toLowerCase()}::${coinModule.toUpperCase()}>`
                    }
                ]
            }
        };
        const results= await suiClient.getOwnedObjects(inputParams);
        results.data.forEach(element => {
            if (element.data?.type?.includes(treasuryCapType)) {
                capObject.treasuryCap = element.data?.objectId;
            }
            if (element.data?.type?.includes(gameAdminCapType)) {
                capObject.gameAdminCap = element.data?.objectId;
            }
        });
    } catch (ex) {
        error = ex;
    }
    callback(error, JSON.stringify(capObject));
}
async function mintInventory(callback: Callback<string>, packageId: string, itemModule: string, coinModule: string, gameAdminCap: string, treasuryCap: string, token: string, items: string, secretKey: string, environment: Environment) {
    let error = null;
    const result = new SuiTransactionResult();
    try {
        const mintRequest: InventoryMintRequest = JSON.parse(items);

        const keypair = Ed25519Keypair.fromSecretKey(fromHEX(secretKey));
        const txb = new TransactionBlock();
        txb.setGasBudget(MIN_GAS_BUDGET);

        if (mintRequest.CurrencyItem != null) {
            const coinTarget: `${string}::${string}::${string}` = `${packageId}::${coinModule.toLowerCase()}::mint`;
            txb.moveCall({
                target: coinTarget,
                arguments: [
                    txb.object(treasuryCap),
                    txb.pure.u64(mintRequest.CurrencyItem.Amount),
                    txb.pure.address(token)
                ]});
        }

        if (mintRequest.GameItems != null) {
            const itemTarget: `${string}::${string}::${string}` = `${packageId}::${itemModule.toLowerCase()}::mint`;
            mintRequest.GameItems.forEach((gameItem) => {
                txb.moveCall({
                    target: itemTarget,
                    arguments: [
                        txb.object(gameAdminCap),
                        txb.pure.address(token),
                        txb.pure.string(gameItem.Name),
                        txb.pure.string(gameItem.Description),
                        txb.pure.string(gameItem.ImageURL)
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

module.exports = {
    exportSecret,
    verifySignature,
    getBalance,
    getOwnedObjects,
    getCapObjects,
    mintInventory
};