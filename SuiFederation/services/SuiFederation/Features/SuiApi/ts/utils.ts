import {InputParams, PaginatedResult} from "./models";

export async function retrievePaginatedData<T, V>(getData: ((input: T) => Promise<V>), input: T) {
    let allResults: V[] = [];
    let nextPage: string | null = null;

    function isResultObject(obj: unknown): obj is PaginatedResult<V> {
        return typeof obj === 'object' && obj !== null;
    }
    function isInputObject(obj: unknown): obj is InputParams {
        return typeof obj === 'object' && obj !== null;
    }

    let data = await getData(input);
    allResults = allResults.concat(data);

    if (isResultObject(data) && 'hasNextPage' in data) {
        const hasNext= data.hasNextPage;
        if (hasNext && 'nextCursor' in data) {
            const nextCursor = data.nextCursor;
            if (nextCursor !== null && nextCursor !== undefined) {
                nextPage = nextCursor;
            }
        }
    }

    while (nextPage != null) {
        if (isInputObject(input) && 'cursor' in input) {
            input.cursor = nextPage;
            data = await getData(input);
            allResults = allResults.concat(data);
            if (isResultObject(data) && 'hasNextPage' in data) {
                const hasNext= data.hasNextPage;
                if (hasNext && 'nextCursor' in data) {
                    const nextCursor = data.nextCursor;
                    if (nextCursor !== null && nextCursor !== undefined) {
                        nextPage = nextCursor;
                    }
                } else {
                    nextPage = null;
                }
            } else {
                nextPage = null;
            }
        }
    }
    return allResults;
}