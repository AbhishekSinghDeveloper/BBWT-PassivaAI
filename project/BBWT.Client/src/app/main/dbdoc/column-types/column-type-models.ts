import { IColumnValidationMetadata, IColumnViewMetadata, AnonymizationRule, ClrTypeGroup } from "../dbdoc-models";


export interface IColumnType {
    id: string;
    name: string;
    anonymizationRule?: AnonymizationRule;
    group: ClrTypeGroup;

    validationMetadataId?: number;
    validationMetadata?: IColumnValidationMetadata;
    viewMetadataId?: number;
    viewMetadata?: IColumnViewMetadata;
}