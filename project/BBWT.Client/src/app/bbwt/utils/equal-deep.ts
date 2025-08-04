//// Makes deep comparing of two objects
export default function equalDeep(x, y) {
    if (x === y) {
        return true; // If both x and y are null or undefined and exactly the same
    } else if (!(x instanceof Object) || !(y instanceof Object)) {
        return false; // If they are not strictly equal, they both need to be Objects
    } else if (x.constructor !== y.constructor) {
        // They must have the exact same prototype chain, the closest we can do is
        // Test their constructor.
        return false;
    } else {
        for (const p in x) {
            if (!x.hasOwnProperty(p)) {
                continue; // Other properties were tested using x.constructor === y.constructor
            }
            if (!y.hasOwnProperty(p)) {
                return false; // Allows to compare x[ p ] and y[ p ] when set to undefined
            }
            if (x[p] === y[p]) {
                continue; // If they have the same strict value or identity then they are equal
            }
            if (typeof (x[p]) !== "object") {
                return false; // Numbers, Strings, Functions, Booleans must be strictly equal
            }
            if (!equalDeep(x[p], y[p])) {
                return false;
            }
        }
        for (const p in y) {
            if (y.hasOwnProperty(p) && !x.hasOwnProperty(p)) {
                return false;
            }
        }
        return true;
    }
}