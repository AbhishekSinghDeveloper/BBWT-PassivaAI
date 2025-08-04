import { Components } from "formiojs";
import ImgUploaderBtnEditDisplay from "./editForm/ImgUploaderBtn.edit.display";

export default function(...extend) {
  return Components.baseEditForm([
    {
      key: "display",
      components: ImgUploaderBtnEditDisplay
    },
    {
      key: "data",
      ignore: true,
    },
    {
      key: "validation",
      ignore: true,
    },
  ], ...extend);
}