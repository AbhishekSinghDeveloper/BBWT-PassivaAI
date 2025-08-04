export interface ILog {
    id: number;
    message: string;
    level: string;
    timeStamp: Date;
    exception: string;
    logEvent: string;
    appName: string;
    server: string;
    iP: string;
    source: string;
    userName: string;
    isImpersonating?: boolean;
    originaluserName: string;
    errorId: string;
    httpStatus?: number;
}

export enum TimeWindow {
    TenMinutes = 0,
    ThirtyMinutes = 1,
    OneHour = 2,
    ThreeHours = 3,
    TwelveHours = 4
  }