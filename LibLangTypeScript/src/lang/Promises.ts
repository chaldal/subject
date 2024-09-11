import { Seq } from "../collections";

export async function inSeries(promiseMakers: Seq<() => Promise<void>>): Promise<void> {
    return inSeriesHelper(promiseMakers, 0);
}

async function inSeriesHelper(promiseMakers: Seq<() => Promise<void>>, startIndex: number): Promise<void> {
    if (promiseMakers.length <= startIndex) return Promise.resolve()
    await promiseMakers[startIndex]();
    await inSeriesHelper(promiseMakers, startIndex + 1);
}