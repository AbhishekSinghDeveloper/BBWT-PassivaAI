import { Formio, Components, Providers } from "formiojs";
import _ from "lodash";

import editForm from "./ImgUploaderBtn.form";

const Field = Components.components.field;
const InputComponent = Components.components.input;

export default class ImageUploaderLauncherComponent extends (Field as any) {

    isReadOnly: boolean = false;
    isPreview: boolean = false;

    static schema(...extend) {
        return InputComponent.schema({
          type: "imageuploader",
          label: "Image Uploader",
          key: "imageuploader",
          action: "Launcher",
          persistent: false,
          dataGridLabel: false
        });
      }

    static get builderInfo() {
        return {
          title: "Image Uploader",
          group: "advanced",
          icon: "image",
          documentation: "/userguide/form-building/form-components#button",
          weight: 130,
          schema: ImageUploaderLauncherComponent.schema()
        };
      }

    public static editForm = editForm;

    constructor(component, options, data) {
        super(component, options, data);

        this.isReadOnly = options.readOnly;
        this.isPreview = options.preview;

        console.log(options)
    }

    /**
     * Called immediately after the component has been instantiated to initialize
     * the component.
     */
    init() {
        super.init();
    }

    get inputInfo() {
        const info = super.elementInfo();
        info.type = "button";
        info.content = this.t(this.component.label, { _userInput: true });
        return info;
      }

      get className() {
        return `${super.className}`;
      }

      labelIsHidden() {
        return true;
      }

      get defaultSchema() {
        return ImageUploaderLauncherComponent.schema();
      }

      render(container) {
        const url = this.getValue() ? this.getValue().url : "";

        const btnRenderer = (!this.isReadOnly || this.isPreview === true) ? super.renderTemplate("button", {
            input: {
                type: "button",
                attr: {
                    class: "btn btn-md btn-info m-1"
                },
                content: '<i class="fa fa-image"></i>  ' + `${this.component.label}`
            },
        }) : "";
        const contentRenderer = this.getValue() ? `<div class="my-auto d-flex overflow-auto"><img style="max-height: 250px;" src="${url}"></img></div>` : "<div></div>";
        const cmpRenderer = `<div class="d-flex flex-column">${contentRenderer}<div>${btnRenderer}</div></div>`;
        return super.render(cmpRenderer);
    }

    attach(element) {
        this.loadRefs(element, {
          customRef: "single",
          buttonMessageContainer: "single",
          buttonMessage: "single"
        });
        const superAttach = super.attach(element);
        this.addEventListener(element.children[0]?.children[1], "click", this.onClick.bind(this));
        return superAttach;
    }

    detach() {
        return super.detach();
    }

    destroy() {
        return super.destroy();
    }

    onClick(event) {
        this.triggerChange();
        event.preventDefault();
    }

    // No label needed for buttons.
  createLabel() {}

  get emptyValue() {
    return null;
  }

  getValue() {
    return this.dataValue;
  }

  get defaultValue() {
    return null;
  }

  setValue(value, flags = {}) {
    if (!value) {
        return;
      }
    const changed = this.updateValue(value, flags);
    if (changed) {
        this.redraw();
    }
    return changed;
  }
}

Components.addComponent("imageuploader", ImageUploaderLauncherComponent);