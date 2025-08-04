export function downloadFileFromBlob(blob: Blob, fileName: string) {
    const link = document.createElement("a");
    link.style.display = "none";
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
}