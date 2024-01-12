import {SuiParsedData} from "@mysten/sui.js/client";
import {DisplayFieldsResponse} from "@mysten/sui.js/src/client/types/generated";

export class SuiBalance {
    coinType: string;
    total: number;
    public constructor(coinType: string, total: number) {
        this.coinType = coinType;
        this.total = total;
    }
}

export class SuiObject {
    objectId: string;
    digest: string;
    version: string;
    content?: SuiParsedData | null;
    display?: DisplayFieldsResponse | null;
    public constructor(objectId: string, digest: string, version: string, content?: SuiParsedData | null, display?: DisplayFieldsResponse | null) {
        this.objectId = objectId;
        this.digest = digest;
        this.version = version;
        this.content = content;
        this.display = display;
    }

    toView(): SuiViewObject {
        const viewObject = new SuiViewObject(this.objectId);

        switch (this.content?.dataType) {
            case 'moveObject':
                viewObject.type = this.content?.type;
                const displayData = this.display?.data;
                if (displayData != null) {
                    viewObject.name = displayData["name"];
                    viewObject.description = displayData["description"];
                    viewObject.image_url = displayData["image_url"];
                }
        }
        return viewObject;
    }
}

export class SuiViewObject {
    objectId: string;
    type: string | undefined;
    name: string | undefined;
    description: string | undefined;
    image_url: string | undefined;
    public constructor(objectId: string) {
        this.objectId = objectId;
    }
}

export interface PaginatedResult<T> {
    data: T[];
    hasNextPage: boolean;
    nextCursor?: string | null;
}
export interface InputParams {
    cursor?: string | null | undefined;
}

export class SuiTransactionResult {
    status: string | undefined;
    digest: string | undefined;
    computationCost: string | undefined;
    objectIds: string[] | undefined;
    error: string | undefined;
}

export class SuiCapObject {
    gameAdminCap: string;
    treasuryCap: string;
    public constructor(gameAdminCap: string, treasuryCap: string) {
        this.gameAdminCap = gameAdminCap;
        this.treasuryCap = treasuryCap;
    }
}

export interface GameItem {
    Name: string;
    Description: string;
    ImageURL: string;
}

export interface CurrencyItem {
    Amount: number;
}

export interface  InventoryMintRequest {
    CurrencyItem: CurrencyItem | undefined;
    GameItems: GameItem[] | undefined;
}

export class SuiKeys {
    Public: string | undefined;
    Private: string | undefined;
}