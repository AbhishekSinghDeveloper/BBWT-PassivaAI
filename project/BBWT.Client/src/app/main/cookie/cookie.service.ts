export class CookieService {
    static GetCookie(cookieName: string): string {
        if (document.cookie) {
            const parsedCookies = document.cookie.split(";").map(x => x.split("="));
            const searchingCookie = parsedCookies.find(x => x[0].trim() === cookieName.trim());
            if (searchingCookie && searchingCookie[1] != null) return searchingCookie[1].trim();
        }

        return null;
    }

    static SetPermanentCookie(cookieName: string, value: string, duration = 94608000000 /* 3 years */, secure: boolean = false): void {
        const currentDate = new Date();
        const o = {
            expires: currentDate,
            domain: "",
            path: "/",
            secure: secure
        };
        o.expires.setTime(currentDate.getTime() + duration);

        document.cookie = cookieName + "=" + encodeURIComponent(value) + "; expires="
            + o.expires.toUTCString() + (o.path ? "; path=" + o.path : "")
            + (o.domain ? "; domain=" + o.domain : "") + (o.secure ? "; secure" : "");
    }
}