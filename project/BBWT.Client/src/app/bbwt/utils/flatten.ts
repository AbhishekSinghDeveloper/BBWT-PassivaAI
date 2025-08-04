import * as moment from "moment";

function getPathKeyFromObject(obj: object, path: string, key: string): string {
    return Array.isArray(obj) ? `${path}[${key}]` : `${path}.${key}`;
}

export const flatten = object => {
    const o = Object.assign({}, ...function _flatten(objectBit, path = "") {
        return [].concat(
            ...Object.keys(objectBit).map(
                key => objectBit[key] && typeof objectBit[key] === "object" && !(objectBit[key] instanceof Date)
                    ? _flatten(
                        objectBit[key],
                        getPathKeyFromObject(objectBit, path, key)
                    )
                    : ({ [getPathKeyFromObject(objectBit, path, key)]: (objectBit[key] == null
                            ? ""
                            : (objectBit[key] instanceof Date
                                ? (objectBit[key] as Date).toUTCString()
                                : encodeURIComponent(objectBit[key]))) })
            )
        );
    }(object));
    return Object.assign({}, ...function _sliceFirst(obj) {
        return [].concat(...Object.keys(o).map(key => ({ [key.slice(1)]: o[key] })));
    }(o));
};

export const flatten1 = (obj: any) => {
    const _flatten = (o: any, path: string) => {};
    return _flatten(obj, "");
};