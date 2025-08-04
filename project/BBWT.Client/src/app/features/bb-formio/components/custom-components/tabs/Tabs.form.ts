//import nestedComponentForm from "../_classes/nested/NestedComponent.form";

// import { Components } from "formiojs";

import nestedComponentForm from "formiojs/components/_classes/nested/NestedComponent.form";

// const nestedComponentForm = Components.components.nested;

import TabsEditDisplay from "./editForm/Tabs.edit.display";

export default function (...extend) {
  return nestedComponentForm([
    {
      key: "display",
      components: TabsEditDisplay
    },
  ], ...extend);
}
