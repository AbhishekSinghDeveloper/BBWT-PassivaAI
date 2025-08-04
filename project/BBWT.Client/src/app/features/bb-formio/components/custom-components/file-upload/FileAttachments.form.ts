import { Components } from "formiojs";

import FileAttachmentsEditData from "./editForm/FileAttachments.edit.data";
import FileAttachmentsEditDisplay from "./editForm/FileAttachments.edit.display";
import FileAttachmentsEditFile from "./editForm/FileAttachments.edit.file";
import FileAttachmentsEditValidation from "./editForm/FileAttachments.edit.validation";


export default function (...extend) {
  return Components.baseEditForm([
    {
      key: "display",
      components: FileAttachmentsEditDisplay
    },
    {
      key: "data",
      components: FileAttachmentsEditData
    },
    {
      key: "validation",
      components: FileAttachmentsEditValidation
    },
    {
      label: "File",
      key: "file",
      weight: 5,
      components: FileAttachmentsEditFile
    },
    {
      key: "api",
      ignore: true
    },
    {
      key: "conditional",
      ignore: true
    },
    {
      key: "logic",
      ignore: true
    },
    {
      key: "layout",
      ignore: true
    }
  ], ...extend);
}

