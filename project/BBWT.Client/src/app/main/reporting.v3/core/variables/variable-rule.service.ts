import {Injectable} from "@angular/core";
import {IEmittedVariable, IVariableRule} from "./variable-models";

@Injectable({
    providedIn: "root"
})
export class VariableRuleService {
    public isMatch(rule: IVariableRule, variable: IEmittedVariable): boolean {
        // If this display rule doesn't match the variable name, return false.
        if (!variable || rule.variableName !== variable.name) return false;

        // TODO: later need to consider value type in comparison. For this phase we take the basic version.
        // Check if the display rule and the variable emitted match, based on display rule operator.
        switch (rule.operator) {
            case "equals":
                return String(variable.value) === String(rule.operand);
            case "notEquals":
                return String(variable.value) !== String(rule.operand);
            case "more":
                return (variable.value as number) > (rule.operand as number);
            case "moreOrEqual":
                return (variable.value as number) >= (rule.operand as number);
            case "less":
                return (variable.value as number) < (rule.operand as number);
            case "lessOrEqual":
                return (variable.value as number) <= (rule.operand as number);
            case "between":
                return (variable.value as number) >= (rule.operand as number)
                    && (variable.value as number) <= (rule.operand as number);
            case "contains":
                return variable.value.includes(rule.operand);
            case "notContains":
                return !variable.value.includes(rule.operand);
            case "startsWith":
                return variable.value.startsWith(rule.operand);
            case "endsWith":
                return variable.value.endsWith(rule.operand);
            case "isSet":
                return !variable.empty;
            default:
                return false;
        }
    }

    public equalVariables(first: IEmittedVariable, second: IEmittedVariable): boolean {
        return first.name === second.name
            && first.value === second.value
            && first.$type === second.$type
            && first.empty === second.empty
            && first.behaviorOnEmpty === second.behaviorOnEmpty;
    }

    public embedVariableValues(source: string, variables: IEmittedVariable[]): string {
        let output: string = source;
        variables?.forEach(variable => output = output.replace(`#${variable.name}`, variable.value));
        return output.replace(/#[0-9a-zA-Z-_]+/g, "");
    }
}
