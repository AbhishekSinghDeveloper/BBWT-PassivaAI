import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { MessageService } from "primeng/api";
import { Message } from "@bbwt/classes";
import { UserService } from "../../users";
import { AdminService } from "../services/admin.service";

@Injectable()
export class AdminGuard  {
    constructor(private router: Router, private userService: UserService, private adminService: AdminService, private messageService: MessageService) { }

    canLoad(): boolean {
        if (this.adminService.accessible) {
            return true;
        } else {
            this.messageService.add(Message.Error("Access denied."));
            this.router.navigate(["/app"]);
            return false;
        }
    }
}