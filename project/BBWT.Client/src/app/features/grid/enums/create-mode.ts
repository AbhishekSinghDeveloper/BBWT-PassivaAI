export enum CreateMode {
    Dialog, // Default
    Disabled,
    External, // The "createFunc" property of the IGridSettings interface is required
    Redirect // The "createLink" parameter of the IGridSettings interface is required
}
