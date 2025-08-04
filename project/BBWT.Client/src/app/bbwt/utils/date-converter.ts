export const SetUTCTimeZone = (date: Date): Date =>
    date ? new Date(date.getTime() - date.getTimezoneOffset() * 60000) : date;

export const SetLocalTimeZone = (date: Date): Date =>
    date ? new Date(date.getTime() + date.getTimezoneOffset() * 60000) : date;