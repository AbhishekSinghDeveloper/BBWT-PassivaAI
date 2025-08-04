export class CellDataTypeInfo {
    constructor(obj?: { min?: number, max?: number, dateFormats?: string, customValidation?: string }) {
        this.min = obj ? obj.min : 20;
        this.max = obj ? obj.max : 70;
        this.dateFormats = obj ? obj.dateFormats : "dd.MM.yyyy, dd/MM/yyyy";
        this.customValidation = obj ? obj.customValidation : "";
    }

    min: number;
    max: number;
    dateFormats: string;
    customValidation: string;
}