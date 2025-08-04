import { Directive, HostBinding, HostListener, EventEmitter, Output, Input } from "@angular/core";

@Directive({
    selector: "[dropZone]"
})
export class DropZoneDirective {
    @Input() multiple = false;
    @Output() private filesChangeEmiter: EventEmitter<FileList> = new EventEmitter();
    @HostBinding("style.background") private background = "#eee";

    @HostListener("dragover", ["$event"]) onDragOver(evt) {
        evt.preventDefault();
        evt.stopPropagation();
        this.background = "#999";
    }

    @HostListener("dragleave", ["$event"]) onDragLeave(evt) {
        evt.preventDefault();
        evt.stopPropagation();
        this.background = "#eee";
    }

    @HostListener("drop", ["$event"]) onDrop(evt) {
        evt.preventDefault();
        evt.stopPropagation();
        const files = evt.dataTransfer.files;
        if (this.multiple || files.length === 1) {
            this.background = "#eee";
            this.filesChangeEmiter.emit(evt);
        } else alert("Multiple addition is disabled");
    }
}