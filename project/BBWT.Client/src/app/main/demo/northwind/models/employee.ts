import { IEntity } from "@bbwt/interfaces/entity";

export interface IEmployee extends IEntity {
    name: string;
    age: number;
    phone: string;
    email: string;
    registrationDate: Date;
    jobRole: string;
}