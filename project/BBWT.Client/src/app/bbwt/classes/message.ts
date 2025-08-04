import {Message as PrimeNgMessage} from "primeng/api";

export class Message implements Message {
    constructor(public severity: string, public summary: string, public detail: string) {}

    static Success(detail: string, summary?: string): PrimeNgMessage {
        return new Message("success", summary || "Success", detail);
    }

    static Error(detail: string, summary?: string): PrimeNgMessage {
        return new Message("error", summary || "Error", detail);
    }

    static Info(detail: string, summary?: string): PrimeNgMessage {
        return new Message("info", summary || "Info", detail);
    }

    static Warning(detail: string, summary?: string): PrimeNgMessage {
        return new Message("warn", summary || "Warning", detail);
    }
}