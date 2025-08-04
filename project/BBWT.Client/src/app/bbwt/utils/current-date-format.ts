import * as moment from "moment";

export const GetCurrentDateFormat = (format: moment.LongDateFormatKey = "L") =>
    moment.localeData(navigator.language).longDateFormat(format);