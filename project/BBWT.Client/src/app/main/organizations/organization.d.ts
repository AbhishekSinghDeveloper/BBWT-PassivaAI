import { IAddress } from "./address";
import { IOrganizationBrand } from "./organization-brand";

export interface IOrganization {
    id: string;
    id_original: number;
    name: string;
    description: string;
    officeCount: number;
    postCode: string;

    addressId?: string;
    address?: IAddress;
    brandingId?: string;
    branding?: IOrganizationBrand;
}