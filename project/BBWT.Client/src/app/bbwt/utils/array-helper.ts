export function removeIf(array: Array<any>, condition: (item: any) => boolean) {
    const index = array.findIndex(condition);
    if (index >= 0) {
        array.splice(index, 1);
    }
}