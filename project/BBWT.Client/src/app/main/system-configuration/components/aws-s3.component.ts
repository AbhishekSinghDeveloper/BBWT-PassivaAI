import { Component } from "@angular/core";

import { AwsStorageService } from "../../aws-storage";



@Component({
    selector: "aws-s3",
    templateUrl: "aws-s3.component.html",
    styleUrls: ["aws-s3.component.scss"]
})
export class AwsS3Component {
    s3Valid = true;
    resultS3Testing: string;
    pending = false;

    constructor(private awsStorageService: AwsStorageService) {}

    testS3Settings(): void {
        this.resultS3Testing = "";
        this.pending = true;
        this.awsStorageService.checkPermissions()
            .then(res => {
                this.resultS3Testing = res.message;
                this.s3Valid = res.success;
            })
            .finally(() => this.pending = false);
    }
}