import { IEntity } from "@bbwt/interfaces/entity";

export interface IDataAudit extends IEntity {
    dateTime: Date;
    stateString: string;
    entityName: string;
    tableName: string;
    userName: string;
    entityId: string;
}