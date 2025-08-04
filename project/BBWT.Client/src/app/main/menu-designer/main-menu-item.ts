interface IMainMenuItem {
    id: number;
    label: string;
    index: number;
    routerLink?: string;
    href?: string;
    classes?: string[];
    icon?: string;
    customHandler?: string;
    hidden?: boolean;
    disabled?: boolean;
    isOpen?: boolean;

    parentId?: number;

    children?: IMainMenuItem[];
}

export { IMainMenuItem };