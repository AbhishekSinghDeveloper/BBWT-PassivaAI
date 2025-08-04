import { Components } from "formiojs";

import BodyMapEditDisplay from "./editForm/BodyMap.edit.display";
import BodyMapEditData from "./editForm/BodyMap.edit.data";
import BodyMapEditValidation from "./editForm/bodymap.edit.validation";

export default function(...extend) {
    return Components.baseEditForm([
      {
        key: "display",
        components: BodyMapEditDisplay
      },
      {
        key: "data",
        components: BodyMapEditData
      },
      {
        key: "validation",
        components: BodyMapEditValidation
      },
    ], ...extend);
}