export interface MessageStatus {
    originalName?: string;
    size?: number;
    progress?: number;
    status?: string;
    message?: string;
}

export interface FileInfo {
    id:           string;
    key:          string;
    thumbnailKey: string;
    url:          string;
    thumbnailUrl: string;
    isImage:      boolean;
    fileName:     string;
    size:         number;
    uploadTime:   Date;
    lastUpdated:  Date;
    id_original:  number;
    originalName: string;
    hash:         string;
}