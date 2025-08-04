import { Injectable } from "@angular/core";
declare let Prism: any;

@Injectable()
export class PrismService {
  init() {
    Prism.highlightAll();
  }
}
