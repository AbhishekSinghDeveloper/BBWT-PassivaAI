export enum UpdateMode {
    Dialog, // Default
    Disabled,
    External, // The "editingFunc" property of the IGridSettings interface is required
    Inline,
    Redirect, // The "editingLink" parameter of the IGridSettings interface is required
}
