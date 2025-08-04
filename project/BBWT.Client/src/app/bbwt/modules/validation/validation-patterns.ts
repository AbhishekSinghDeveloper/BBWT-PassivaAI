export enum ValidationPatterns {
    email = "^[\\w-.]+(\\+[\\w-.]+)?@[\\w-.]+\\.[\\w-]{2,}$",
    phone = "(\\+?\\d[- .]*){7,13}",
    floatNumber = "^-?(0|[1-9]\\d*)(\\.\\d+)?$",
    notEmpty = "^(?!\\s+$).+",
    uri = "^[A-Za-z0-9\\._~:\\/\\?#\\[\\]@!\\$&'\\(\\)\\*\\+\\-,;=]*$" /* https://datatracker.ietf.org/doc/html/rfc3986/ */
}