export interface LoginRequest {
    userName: string;
    email: string;
    password: string;
    captchaResponse: string;
    fingerprint: string;
    browser: string;
    twoFactorCode: string;
    twoFactorRecoveryCode: string;
    realFirstName: string;
    realLastName: string;
    realEmail: string;
    pinHash: string;
}
