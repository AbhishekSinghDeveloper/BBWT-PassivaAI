import { FileDetails } from "../file-storage/file-details";
import { IOrganization } from "./organization";

export interface IOrganizationBrand {
    id: number;
    theme: string;
    disabled: boolean;
    emailBody?: string;

    organization: IOrganization;

    logoImageId: string;
    logoImage: FileDetails;
    logoIconId: string;
    logoIcon: FileDetails;
}