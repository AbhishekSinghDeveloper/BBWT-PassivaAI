import {IEmittedVariable} from "./variable-models";

export interface IVariableReceiver {
    variableReceiverId: string;

    receiveEmittedVariables(variables: IEmittedVariable[]): void;
}
