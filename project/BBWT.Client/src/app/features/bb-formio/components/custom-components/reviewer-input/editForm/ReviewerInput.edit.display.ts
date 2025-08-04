import { FormioUtils } from "@formio/angular";
import { Builders, ExtendedComponentSchema, Utils } from "formiojs";
import _ from "lodash";

export default [
  {
    weight: 10,
    type: "select",
    input: true,
    key: "targetTab",
    label: "Target Tab",
    tooltip: "The tab to which this field will point.",
    dataSrc: "custom",
    data: {
      custom(context) {       
        const getInnerComponents = (component) => {
          let children = component["components"] ?? component["columns"];
          const rows = component["rows"];
          if (!children && Array.isArray(rows)) {
            children = [];
            rows?.forEach(row => {
              children = children.concat(...row);
            });
          }
          console.log(children);
          let result: any;
          if (component.type == "state-tabs") return component;
          else if (children) {
              children.forEach(element => {
                  // Call recursively to process inner components
                  const item = getInnerComponents(element);
                  if (item) result = item;
              });              
          }
          return result;
        }

        const editForm = getInnerComponents(_.get(context, "instance.options.editForm", {}));

        if(editForm) {
          return _.map(editForm.components, (tab) => ({
            label: tab.label,
            key: tab.key
          }));
        }
        return null;
      },
    },
  },
  {
    weight: 400,
    type: "select",
    input: true,
    key: "widget.type",
    label: "Widget",
    placeholder: "Select a widget",
    tooltip: "The widget is the display UI used to input the value of the field.",
    defaultValue: "input",
    onChange: (context) => {
      context.data.widget = _.pick(context.data.widget, "type");
    },
    dataSrc: "values",
    data: {
      values: [
        { label: "Input Field", value: "input" },
        { label: "Calendar Picker", value: "calendar" },
      ]
    },
    conditional: {
      json: { "===": [{ var: "data.type" }, "textfield"] }
    }
  },
  {
    weight: 410,
    type: "select",
    input: true,
    key: "applyMaskOn",
    label: "Apply Mask On",
    tooltip: "Select the type of applying mask.",
    defaultValue: "change",
    dataSrc: "values",
    data: {
      values: [
        { label: "Change", value: "change" },
        { label: "Blur", value: "blur" },
      ],
    },
    customConditional(context) {
      return !context.data.allowMultipleMasks;
    },
  },
  {
    weight: 413,
    type: "checkbox",
    input: true,
    key: "allowMultipleMasks",
    label: "Allow Multiple Masks"
  },
  {
    weight: 1350,
    type: "checkbox",
    input: true,
    key: "spellcheck",
    defaultValue: true,
    label: "Allow Spellcheck"
  },
  {
    weight: 320,
    type: "textfield",
    input: true,
    key: "prefix",
    label: "Prefix",
    ignore: true
  },
  {
    weight: 330,
    type: "textfield",
    input: true,
    key: "suffix",
    label: "Suffix"
  },
  {
    weight: 700,
    type: "textfield",
    input: true,
    key: "autocomplete",
    label: "Autocomplete",
    placeholder: "on",
    tooltip: "Indicates whether input elements can by default have their values automatically completed by the browser. See the <a href=\"https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes/autocomplete\">MDN documentation</a> on autocomplete for more information."
  },
  {
    weight: 1300,
    type: "checkbox",
    label: "Hide Input",
    tooltip: "Hide the input in the browser. This does not encrypt on the server. Do not use for passwords.",
    key: "mask",
    input: true
  },
  {
    weight: 1200,
    type: "checkbox",
    label: "Show Word Counter",
    tooltip: "Show a live count of the number of words.",
    key: "showWordCount",
    input: true
  },
  {
    weight: 1201,
    type: "checkbox",
    label: "Show Character Counter",
    tooltip: "Show a live count of the number of characters.",
    key: "showCharCount",
    input: true
  },
];
