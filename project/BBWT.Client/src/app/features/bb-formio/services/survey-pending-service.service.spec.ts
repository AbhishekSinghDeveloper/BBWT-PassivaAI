import { TestBed } from "@angular/core/testing";

import { SurveyPendingServiceService } from "./survey-pending-service.service";

describe("SurveyPendingServiceService", () => {
  let service: SurveyPendingServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SurveyPendingServiceService);
  });

  it("should be created", () => {
    expect(service).toBeTruthy();
  });
});
