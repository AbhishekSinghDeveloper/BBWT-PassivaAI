import { Components } from "formiojs";

import ReviewerInputEditData from "./editForm/ReviewerInput.edit.data";
import ReviewerInputEditDisplay from "./editForm/ReviewerInput.edit.display";
import ReviewerInputEditValidation from "./editForm/ReviewerInput.edit.validation";

export default function(...extend) {
  return Components.baseEditForm([
    {
      key: "display",
      components: ReviewerInputEditDisplay
    },
    {
      key: "data",
      components: ReviewerInputEditData
    },
    {
      key: "validation",
      components: ReviewerInputEditValidation
    }
  ]);
}
