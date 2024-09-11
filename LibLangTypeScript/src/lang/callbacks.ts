export type CompletionCallback = (error?: any) => void;
export type ResultCallback<T> = {
    (error: any) : void;
    (error: undefined, result: T) : void;
}

import { Seq } from "../collections";

export function inSeries(cbAbles: Seq<CompletionCallback>, cb: CompletionCallback) : void {
    if (cbAbles.length === 0) {
        cb();
        return;
    }

    cbAbles[0]((err: any) => {
        if (err) {
            cb(err);
        }
        else {
            inSeries(cbAbles.slice(1), cb);
        }
    });
}
