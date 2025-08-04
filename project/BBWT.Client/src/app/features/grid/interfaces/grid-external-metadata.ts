import { GridColumnViewSettings } from "../classes/grid-column-view-settings";
import { GridValidator } from "../classes/grid-validator";

export interface IGridExternalMetadata {
    [field: string]: { validators: GridValidator[], viewSettings: GridColumnViewSettings } ;
}