import { StrongPasswordValidatorDirective } from "./strong-password.directive";

describe("StrongPasswordDirective", () => {
    it("should create an instance", () => {
        const directive = new StrongPasswordValidatorDirective(null);
        expect(directive).toBeTruthy();
    });
});