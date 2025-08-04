import {Injectable} from "@angular/core";
import {IVariableEmitter} from "./variable-emitter";
import {IVariableReceiver} from "./variable-receiver";
import {IEmittedVariable} from "./variable-models";
import {IHash} from "@bbwt/interfaces";

@Injectable({
    providedIn: "root"
})
export class VariableHubService {
    private isEmitting: boolean = false;
    private emissionQueue: (() => void)[] = [];
    private variableEmitters: IVariableEmitter[] = [];
    private variableReceivers: IVariableReceiver[] = [];
    private lastEmittedVariablesByEmitter: IHash<IEmittedVariable[]> = {};


    public registerVariableEmitter(variableEmitter: IVariableEmitter): void {
        // If this emitter is registered, return.
        if (this.variableEmitters.some(emitter => emitter.variableEmitterId === variableEmitter.variableEmitterId)) return;

        // Otherwise, register the emitter.
        this.variableEmitters.push(variableEmitter);

        // Register this emitter's variable list in the dictionary of emitted variables.
        this.lastEmittedVariablesByEmitter[variableEmitter.variableEmitterId] = [];
    }

    public unregisterVariableEmitter(variableEmitter: IVariableEmitter): void {
        // If this emitter is not registered, return.
        if (!this.variableEmitters.some(emitter => emitter.variableEmitterId === variableEmitter.variableEmitterId)) return;

        // Otherwise, unregister the emitter.
        this.variableEmitters = this.variableEmitters.filter(emitter => emitter.variableEmitterId !== variableEmitter.variableEmitterId);

        // Unregister this emitter's variable list in the dictionary of emitted variables.
        this.lastEmittedVariablesByEmitter[variableEmitter.variableEmitterId] = undefined;

        // Re-emit remaining variables.
        this.emitVariablesToReceivers(this.variableReceivers);
    }

    public registerVariableReceiver(variableReceiver: IVariableReceiver): void {
        // If this receiver is registered, return.
        if (this.variableReceivers.some(receiver => receiver.variableReceiverId === variableReceiver.variableReceiverId)) return;

        // Otherwise, register the receiver.
        this.variableReceivers.push(variableReceiver);

        // Emit new state of variables to this receiver.
        this.emitVariablesToReceivers([variableReceiver]);
    }

    public unregisterVariableReceiver(variableReceiver: IVariableReceiver): void {
        // If this receiver is not registered, return.
        if (!this.variableReceivers.some(receiver => receiver.variableReceiverId === variableReceiver.variableReceiverId)) return;

        // Otherwise, unregister the receiver.
        this.variableReceivers = this.variableReceivers.filter(receiver => receiver.variableReceiverId !== variableReceiver.variableReceiverId);
    }

    public emitVariables(variableEmitterId: string, variables: IEmittedVariable[], ...excludeReceiverIds: string[]): void {
        // If this emitter id is not registered, return.
        if (!this.variableEmitters.some(emitter => emitter.variableEmitterId === variableEmitterId)) return;

        // Remove previously emitted variables with the same name as given variables, to avoid emission of duplicated variables.
        Object.keys(this.lastEmittedVariablesByEmitter).forEach(emitterId =>
            this.lastEmittedVariablesByEmitter[emitterId] = this.lastEmittedVariablesByEmitter[emitterId]
                ?.filter(emittedVariable => !variables.some(variable => variable.name === emittedVariable.name)));

        // Substitute the last emitted variables of this emitter for the current ones.
        this.lastEmittedVariablesByEmitter[variableEmitterId] = variables;

        // Exclude from receivers the ones whose id is in the list of exclusions.
        const variableReceivers = this.variableReceivers
            .filter(variableReceiver => !excludeReceiverIds?.includes(variableReceiver.variableReceiverId));

        // Emit new state of variables to all receivers.
        this.emitVariablesToReceivers(variableReceivers);
    }

    private emitVariablesToReceivers(variableReceivers: IVariableReceiver[]): void {
        // Get the last emitted variables from all emitters.
        const lastEmittedVariables: IEmittedVariable[] = Object.keys(this.lastEmittedVariablesByEmitter)
            .flatMap(emitterId => this.lastEmittedVariablesByEmitter[emitterId] ?? []);

        // Add this emission request to the emission queue.
        this.emissionQueue.push(() => variableReceivers.forEach(variableReceiver => variableReceiver.receiveEmittedVariables(lastEmittedVariables)));

        // If queue is not emitting, start emission.
        if (!this.isEmitting) this.processNextEmission();
    }

    private processNextEmission(): void {
        // Mark as emitting only if there are more emissions in the queue.
        this.isEmitting = this.emissionQueue.length > 0;

        if (!this.isEmitting) return;

        // Get the first emission from the queue.
        const currentEmission = this.emissionQueue.shift();

        // Execute the emission.
        if (currentEmission) currentEmission();

        // Continue with the next emission.
        this.processNextEmission();
    }
}
