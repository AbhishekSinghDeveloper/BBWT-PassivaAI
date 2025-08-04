import * as moment from "moment";

export function displayUtc(value: moment.Moment): string {
    const dt = moment.utc(value);
    if (dt.isValid()) {
        return dt.format("L LTS");
    }

    return null;
}

export function displayDuration(startTime: moment.Moment, endTime: moment.Moment): string {
    function r(x: number): number {
        return Math.round(x * 100) / 100;
    }

    const start = moment.utc(startTime);
    const end = moment.utc(endTime);

    if (start.isValid() && end.isValid()) {
        const duration = moment.duration(end.diff(start));

        if (duration.asSeconds() < 1) {
            return `${r(duration.asMilliseconds())}ms`;
        }

        if (duration.asMinutes() < 1) {
            return `${r(duration.asSeconds())}s`;
        }

        if (duration.asHours() < 1) {
            return `${r(duration.asMinutes())}m`;
        }

        return duration.humanize();
    }

    return null;
}
