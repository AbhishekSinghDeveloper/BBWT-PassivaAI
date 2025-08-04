export interface U2fAuthenticationRequest {
    appId: string;
    version: string;
    challenges: string;
    challenge: string;
}