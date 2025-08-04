// TODO: show image not the list of links
// ${ctx.files.map(file => `
// <div>
//     <span>
//         <img ref="fileImage" src="" alt="${file.originalName || file.fileName}" style="width:${ctx.component.imageSize}px">
//         ${!ctx.disabled ? `<i class="${ctx.iconClass("remove")}" ref="removeLink"></i>` : ""}
//     </span>
// </div>
// `).join("")}

export default function attachmentsTemplate(ctx: any): string {
    return `
        <img ref="img" style="display: none;" id="usedForResizingImages" />
        
        <div>
            <ul class="list-group list-group-striped">
                <li class="list-group-item list-group-header hidden-xs hidden-sm">
                    <div class="row">
                    
                        <div class="col-md-1"></div>
                    
                        <div class="col-md-9"><strong>File Name</strong></div>
                        <div class="col-md-2"><strong>Size</strong></div>

                     </div>
                </li>

                ${ctx.files.map(file =>
                     `
                    <li class="list-group-item">
                    <div class="row">
        
                    ${!ctx.disabled
                        ? `
                        <div class="col-md-1">
                            <i ref="removeLink" class="fa fa-remove" tabindex="0"></i>
                        </div>`
                        : `
                        <div class="col-md-1"></div>`
                    }
        
                        <div class="col-md-9">
                            <a ref="fileLink" target="_blank" style="color: blue; text-decoration: underline;" href="${file.url ?? ""}">
                                <span class="sr-only">Press to open </span>${file.originalName || file.fileName}
                            </a>
                        </div>
                    
                        <div class="col-md-2">${ctx.fileSize(file.size)}</div>
                    </div>
                </li>
                `).join("")}
                
            </ul>
        </div>

        ${!ctx.disabled && (ctx.component.multiple || !ctx.files.length) ? `
            ${ctx.self.useWebViewCamera ? `
                <div class="fileSelector">
                    <button class="btn btn-primary" ref="galleryButton"><i class="fa fa-book"></i> ${ctx.t("Gallery")}</button>
                    <button class="btn btn-primary" ref="cameraButton"><i class="fa fa-camera"></i> ${ctx.t("Camera")}</button>
                </div>
            ` : !ctx.self.cameraMode ? `
                <div class="fileSelector" ref="fileDrop" ${ctx.fileDropHidden ? "hidden" : ""}>
                    <i class="${ctx.iconClass("cloud-upload")}"></i> ${ctx.t("Drop files to attach,")}
                    ${ctx.self.imageUpload ? `<a href="#" ref="toggleCameraMode"><i class="fa fa-camera"></i> ${ctx.t("Use Camera,")}</a>` : ""}
                    ${ctx.t("or")} <a href="#" ref="fileBrowse" class="browse">${ctx.t("browse")}</a>
                </div>
            ` : `
                <div>
                    <video class="video" autoplay="true" ref="videoPlayer"></video>
                </div>
                <button class="btn btn-primary" ref="takePictureButton"><i class="fa fa-camera"></i> ${ctx.t("Take Picture")}</button>
                <button class="btn btn-primary" ref="toggleCameraMode">${ctx.t("Switch to file upload")}</button>
            `} 
        ` : ""}
        
        ${ctx.statuses.map(status => `
            <div class="file ${status.status === "error" ? " has-error" : ""}">
                <div class="row">
                    <div class="fileName col-form-label col-sm-10">${status.originalName}
                        <i class="fa fa-remove" ref="fileStatusRemove"></i>
                    </div>
                    <div class="fileSize col-form-label col-sm-2 text-right">${ctx.fileSize(status.size)}</div>
                </div>
                <div class="row">
                    <div class="col-sm-12">
                        ${status.status === "progress" ? `
                            <div class="progress">
                                <div class="progress-bar" role="progressbar" aria-valuenow="${status.progress}" aria-valuemin="0" aria-valuemax="100" style="width: ${status.progress}%">
                                    <span class="sr-only">${status.progress}% Complete}</span>
                                </div>
                            </div>
                        ` : status.status === "error" ? `
                            <div class="alert alert-danger bg-${status.status}">${status.message}</div>
                        ` : `
                            <div class="bg-${status.status}">${status.message}</div>
                        `}
                    </div>
                </div>
            </div>
        `).join("")}
        
        ${!ctx.component.storage || ctx.support.hasWarning ? `
            <div class="alert alert-warning">
                ${!ctx.component.storage ? `<p>${ctx.t("No storage has been set for this field. File uploads are disabled until storage is set up.")}</p>` : ""}
                ${!ctx.support.filereader ? `<p>${ctx.t("File API & FileReader API not supported.")}</p>` : ""}
                ${!ctx.support.formdata ? `<p>${ctx.t('XHR2"s FormData is not supported.')}</p>` : ""}
                ${!ctx.support.progress ? `<p>${ctx.t('XHR2"s upload progress isn"t supported.')}</p>` : ""}
            </div>
        ` : ""}
    `;
}
