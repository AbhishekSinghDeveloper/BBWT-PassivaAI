export interface U2fAuthenticationResponse {
    userId: string;
    clientData: string;
    keyHandle: string;
    signatureData: string;
}