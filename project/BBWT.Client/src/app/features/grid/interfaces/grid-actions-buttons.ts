/** Defines an additional action for each record. These actions will be displayed inside the "Actions" column. */
export class IGridActionsRowButton {

    /** The CSS class of the button. */
    buttonClass?: string;

    /** Defines a function that determines whether the button is disabled based on the record value. */
    disabled?: (rowData: any) => boolean;

    /** Defines an "onClick" event handler. */
    handler: (rowData: any) => void;

    /** Defines an icon from Material. The full list is here https://fonts.google.com/icons */
    materialIcon?: string;

    /** Defines a text inside the button. */
    label?: string;

    hint?: string;

    /** Defines an icon from PrimeNG. The full list is here https://www.primefaces.org/diamond/icons.xhtml */
    primeIcon?: string;

    /** Defines a function that determines whether the button is visible based on the record value. */
    visible?: (rowData: any) => boolean;

}

/** Defines an additional action for the entire table. These actions will be displayed at the bottom of the table. */
export class IGridActionsButton {

    /** The CSS class of the button. */
    buttonClass?: string;

    /** Defines a function that determines whether the button is disabled. */
    disabled?: () => boolean;

    /** Defines an "onClick" event handler. */
    handler: () => void;

    /** Defines an icon from Material. The full list is here https://fonts.google.com/icons */
    materialIcon?: string;

    /** Defines a text inside the button. */
    label?: string;

    /** Defines an icon from PrimeNG. The full list is here https://www.primefaces.org/diamond/icons.xhtml */
    primeIcon?: string;

    /** Defines a function that determines whether the button is visible. */
    visible?: () => boolean;

}