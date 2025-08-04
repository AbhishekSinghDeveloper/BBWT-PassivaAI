import { AbstractControl, ValidationErrors } from "@angular/forms";
import * as awsCronParser from "aws-cron-parser";
import { SetUTCTimeZone } from "@bbwt/utils";

type CronField = "Minutes" | "Hours" | "Day-of-Month" | "Month" | "Day-of-Week" | "Year";

const monthToNumber = [
    ["JAN", "1"],
    ["FEB", "2"],
    ["MAR", "3"],
    ["APR", "4"],
    ["MAY", "5"],
    ["JUN", "6"],
    ["JUL", "7"],
    ["AUG", "8"],
    ["SEP", "9"],
    ["OCT", "10"],
    ["NOV", "11"],
    ["DEC", "12"]
];

const dayWeekToNumber = [
    ["SUN", "1"],
    ["MON", "2"],
    ["TUE", "3"],
    ["WED", "4"],
    ["THU", "5"],
    ["FRI", "6"],
    ["SAT", "7"]
];

const minMaxMap = {
    Minutes: [0, 59],
    Hours: [0, 23],
    "Day-of-Month": [1, 31],
    Month: [1, 12],
    "Day-of-Week": [1, 7],
    Year: [1970, 2199]
};

const fieldValidators = [
    createValidator("Minutes"),
    createValidator("Hours"),
    createValidator("Day-of-Month"),
    createValidator("Month"),
    createValidator("Day-of-Week"),
    createValidator("Year")
];

function createValidator(field: CronField) {
    const [min, max] = minMaxMap[field];
    const nameToNumber =
        field === "Month" ? monthToNumber : field === "Day-of-Week" ? dayWeekToNumber : null;
    return (fieldValue: string): string => {
        if (fieldValue === "*" || fieldValue === "?") {
            return getValues(min, 1, max).join(",");
        }

        if (nameToNumber != null) {
            nameToNumber.forEach(
                ([name, num]) => (fieldValue = fieldValue.replace(new RegExp(name, "g"), `${num}`))
            );
        }

        if ((field === "Day-of-Month" || field === "Day-of-Week") && fieldValue.indexOf("L") >= 0) {
            if (fieldValue !== "L") {
                throw new Error();
            }
            return fieldValue;
        }

        if (field === "Day-of-Month" && fieldValue.indexOf("W") >= 0) {
            if (!fieldValue.match(/^[1-7]W$/)) {
                throw new Error();
            }
            return fieldValue;
        }

        if (field === "Day-of-Week" && fieldValue.indexOf("#") >= 0) {
            if (!fieldValue.match(/^[1-7]#[1-5]$/)) {
                throw new Error();
            }
            return fieldValue;
        }

        const values: number[] = [];
        for (const part of fieldValue.split(",")) {
            if (part === "" || part === "*" || part === "?") {
                throw new Error();
            }

            if (part.indexOf("/") >= 0) {
                if (field === "Day-of-Week") {
                    throw new Error();
                }
                const inc = part.split("/");
                let _s = inc[0];
                const _i = inc[1];
                if (_s === "*") {
                    _s = `${min}`;
                }
                const [s, i] = [parseIntEx(_s), parseIntEx(_i)];
                if (i <= 0 || s < min || s > max) {
                    throw new Error();
                }
                values.push(...getValues(s, i, max));
            } else if (part.indexOf("-") >= 0) {
                const [_s, _e] = part.split("-");
                const [s, e] = [parseIntEx(_s), parseIntEx(_e)];
                if (s < min || e > max || s > e) {
                    throw new Error();
                }
                values.push(...getValues(s, 1, e));
            } else {
                const v = parseIntEx(part);
                if ((v < min || v > max) && field !== "Year") {
                    throw new Error();
                }
                values.push(v);
            }
        }

        return values
            .sort((n1, n2) => n1 - n2)
            .reduce((acc, xi) => (acc.includes(xi) ? acc : [...acc, xi]), [])
            .join(",");
    };
}

function parseIntEx(value: string): number {
    if (!value.match(/^(0|[1-9]\d*)$/)) {
        throw new Error();
    }

    const result = +value;
    if (isNaN(result)) {
        throw new Error(); // Just in case, we should never reach this line
    }

    return result;
}

function getValues(start: number, increment: number, max: number): number[] {
    return [...Array(max - start + 1)].map((_, i) => start + i * increment).filter((x) => x <= max);
}

function ValidateDays(dayOfMonth: string, dayOfWeek: string) {
    [
        [dayOfMonth, dayOfWeek],
        [dayOfWeek, dayOfMonth]
    ].forEach(([d1, d2]) => {
        if ((d1.indexOf("*") >= 0 || d1 !== "?") && d2 !== "?") {
            throw new Error();
        }
    });
}

export function awsCronValidator(c: AbstractControl): ValidationErrors | null {
    const value = c.value;
    if (typeof value === "string" && value != null && value !== "") {
        try {
            if (!value.match(/^([^\s]+\s){5}[^\s]+$/)) {
                throw new Error();
            }
            let fields = value.split(" ");

            ValidateDays(fields[2], fields[4]);
            fields = fields.map((f, i) => fieldValidators[i](f));

            const cron = awsCronParser.parse(fields.join(" "));
            const nowUtc = SetUTCTimeZone(new Date());
            const next = awsCronParser.next(cron, nowUtc);
            if (next == null) {
                throw new Error();
            }
        } catch (e: unknown) {
            return {
                cron: true
            };
        }
    }

    return null;
}
