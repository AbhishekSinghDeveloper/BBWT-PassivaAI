import { Message } from "./index";

describe("Classes Index", () => {
    describe("Message Export", () => {
        it("should export Message class", () => {
            expect(Message).toBeDefined();
            expect(typeof Message).toBe("function");
        });

        it("should be able to create Message instance", () => {
            const message = new Message("info", "Test Summary", "Test Detail");
            expect(message).toBeTruthy();
            expect(message.severity).toBe("info");
            expect(message.summary).toBe("Test Summary");
            expect(message.detail).toBe("Test Detail");
        });
    });

    describe("Message Class Functionality", () => {
        describe("Constructor", () => {
            it("should create instance with all parameters", () => {
                const message = new Message("error", "Error Summary", "Error Detail");
                
                expect(message.severity).toBe("error");
                expect(message.summary).toBe("Error Summary");
                expect(message.detail).toBe("Error Detail");
            });
        });

        describe("Static Methods", () => {
            describe("Success", () => {
                it("should create success message with custom summary", () => {
                    const message = Message.Success("Operation completed", "Custom Success");
                    
                    expect(message.severity).toBe("success");
                    expect(message.summary).toBe("Custom Success");
                    expect(message.detail).toBe("Operation completed");
                });

                it("should create success message with default summary", () => {
                    const message = Message.Success("Operation completed");
                    
                    expect(message.severity).toBe("success");
                    expect(message.summary).toBe("Success");
                    expect(message.detail).toBe("Operation completed");
                });
            });

            describe("Error", () => {
                it("should create error message with custom summary", () => {
                    const message = Message.Error("Operation failed", "Custom Error");
                    
                    expect(message.severity).toBe("error");
                    expect(message.summary).toBe("Custom Error");
                    expect(message.detail).toBe("Operation failed");
                });

                it("should create error message with default summary", () => {
                    const message = Message.Error("Operation failed");
                    
                    expect(message.severity).toBe("error");
                    expect(message.summary).toBe("Error");
                    expect(message.detail).toBe("Operation failed");
                });
            });

            describe("Info", () => {
                it("should create info message with custom summary", () => {
                    const message = Message.Info("Information message", "Custom Info");
                    
                    expect(message.severity).toBe("info");
                    expect(message.summary).toBe("Custom Info");
                    expect(message.detail).toBe("Information message");
                });

                it("should create info message with default summary", () => {
                    const message = Message.Info("Information message");
                    
                    expect(message.severity).toBe("info");
                    expect(message.summary).toBe("Info");
                    expect(message.detail).toBe("Information message");
                });
            });

            describe("Warning", () => {
                it("should create warning message with custom summary", () => {
                    const message = Message.Warning("Warning message", "Custom Warning");
                    
                    expect(message.severity).toBe("warn");
                    expect(message.summary).toBe("Custom Warning");
                    expect(message.detail).toBe("Warning message");
                });

                it("should create warning message with default summary", () => {
                    const message = Message.Warning("Warning message");
                    
                    expect(message.severity).toBe("warn");
                    expect(message.summary).toBe("Warning");
                    expect(message.detail).toBe("Warning message");
                });
            });
        });
    });
});