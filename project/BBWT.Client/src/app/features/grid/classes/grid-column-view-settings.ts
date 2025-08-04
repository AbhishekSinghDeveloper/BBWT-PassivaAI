export class GridColumnViewSettings {
    minWidth: number | string;
    maxWidth: number | string;
    width: number | string;


    constructor(data: any) {
        if (data) {
            Object.keys(data).forEach(keyItem => this[keyItem] = data[keyItem]);
        }
    }


    getStyles(): {[key: string]: string} {
        const result = {};

        if (this.minWidth != null) {
            result["min-width"] = typeof this.minWidth == "string"
                ? <string> this.minWidth
                : <number> this.minWidth + "px";
        }
        if (this.maxWidth != null) {
            result["max-width"] = typeof this.maxWidth == "string"
                ? <string> this.maxWidth
                : <number> this.maxWidth + "px";
        }
        if (this.width != null) {
            result["width"] = typeof this.width == "string"
                ? <string> this.width
                : <number> this.width + "px";
        }

        return result;
    }
}
