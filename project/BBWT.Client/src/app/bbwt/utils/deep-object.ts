export function isObject(item) {
    return (item && typeof item === "object" && !Array.isArray(item) && !(item instanceof Date));
}

//// Makes a deep merge of two objects. The result is a new object.
export function mergeDeep(target: any, source: any): any {
    const output = Object.assign({}, target);
    if (isObject(target) && isObject(source)) {
        Object.keys(source).forEach(key => {
            if (isObject(source[key])) {
                if (!(key in target)) {
                    Object.assign(output, { [key]: source[key] });
                } else {
                    output[key] = mergeDeep(target[key], source[key]);
                }
            } else {
                if (source[key] instanceof Date) {
                    Object.assign(output, { [key]: new Date(source[key]) });
                } else if (Array.isArray(source[key])) {
                    const array = (source[key] as Array<any>).map(x => mergeDeep({}, x));
                    Object.assign(output, { [key]: array });
                } else {
                    Object.assign(output, { [key]: source[key] });
                }
            }
        });
    }
    return output;
}

function deepUpdateProperty(target: any, source: any, propertyName: any) {
    if (isObject(source[propertyName])) {
        if (target[propertyName] == null) {
            target[propertyName] = {};
        }

        deepUpdate(target[propertyName], source[propertyName]);
    } else {
        if (source[propertyName] instanceof Date) {
            Object.assign(target, { [propertyName]: new Date(source[propertyName]) });
        } else if (Array.isArray(source[propertyName])) {
            if (!Array.isArray(target[propertyName])) {
                target[propertyName] = [];
            }

            deepUpdate(target[propertyName], source[propertyName]);
        } else {
            Object.assign(target, { [propertyName]: source[propertyName] });
        }
    }
}

//// Performs a copy of the object without the references changing.
export function deepUpdate(target: any, source: any): void {
    if (isObject(target) && isObject(source)) {
        Object.keys(target).filter(x => !(x in source)).forEach(key => delete target[key]);
        Object.keys(source).forEach(key => deepUpdateProperty(target, source, key));
    }
    if (Array.isArray(target) && Array.isArray(source)) {
        const targetArray = target as any[];
        const sourceArray = source as any[];

        if (targetArray.length > sourceArray.length) {
            targetArray.splice(sourceArray.length);
        }

        for (let index = 0; index < sourceArray.length; index++) {
            if (targetArray.length <= index) {
                targetArray.push({});
            }

            deepUpdateProperty(targetArray, sourceArray, index);
        }
    }
}