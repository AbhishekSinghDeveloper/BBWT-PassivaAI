import { PasswordFeedbackComponent } from "./password-feedback.component";
import { ComponentFixture, async, TestBed, waitForAsync } from "@angular/core/testing";

describe("PasswordFeedbackComponent", () => {
    let component: PasswordFeedbackComponent;
    let fixture: ComponentFixture<PasswordFeedbackComponent>;

    beforeEach(waitForAsync(() => {
        TestBed.configureTestingModule({
            declarations: [PasswordFeedbackComponent]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(PasswordFeedbackComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it("should create", () => {
        expect(component).toBeTruthy();
    });
});
