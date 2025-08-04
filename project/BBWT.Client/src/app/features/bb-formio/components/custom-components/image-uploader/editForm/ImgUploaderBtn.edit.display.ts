import _ from "lodash";

export default [
  {
    key: "labelPosition",
    ignore: true,
  },
  {
    key: "placeholder",
    ignore: true,
  },
  {
    key: "hideLabel",
    customConditional(context) {
      return context.instance.options?.flags?.inDataGrid;
    },
  },
  {
    key: "dataGridLabel",
    ignore: true,
  },
  {
    type: "select",
    key: "action",
    label: "Action",
    input: true,
    dataSrc: "values",
    weight: 110,
    tooltip: "This is the action to be performed by this button.",
    data: {
      values: [
        { label: "Image Upload Launcher", value: "Launcher" }
      ],
    },
  }
];