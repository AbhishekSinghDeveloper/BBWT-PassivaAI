import { Formio, Components, Providers } from "formiojs";
import SignaturePad from "signature_pad";
import _ from "lodash";

import editForm from "./BodyMap.form";
import { bodyImageFrontAndBack } from "./fixtures/bodymapFrontAndBack";

const InputComponent = Components.components.input;


export default class BodyMapComponent extends (InputComponent as any) {

  // container of BodyMap
  formioForm = null;

  /**
   * This is the default schema of your custom component. It will "derive"
   * from the base class "schema" and extend it with its default JSON schema
   * properties. The most important are "type" which will be your component
   * type when defining new components.
   *
   * @param extend - This allows classes deriving from this component to
   *                 override the schema of the overridden class.
   */
  static schema() {
    return InputComponent.schema({
      type: "bodymap",
      label: "Body Map",
      key: "bodymap",
      width: "300px",
      height: "400px",
      penColor: "red",
      backgroundColor: "rgb(245, 245, 235)",
      minWidth: "0.5",
      maxWidth: "2.5",
      keepOverlayRatio: true
    });
  }

  public static editForm = editForm;

  /**
   * This is the Form Builder information on how this component should show
   * up within the form builder. The "title" is the label that will be given
   * to the button to drag-and-drop on the buidler. The "icon" is the font awesome
   * icon that will show next to it, the "group" is the component group where
   * this component will show up, and the weight is the position within that
   * group where it will be shown. The "schema" field is used as the default
   * JSON schema of the component when it is dragged onto the form.
   */
  static get builderInfo() {
    return {
      title: "Body Map",
      group: "advanced",
      icon: "stethoscope",
      weight: 120,
      documentation: "/developers/integrations/esign/esign-integrations#signature-component",
      schema: BodyMapComponent.schema()
    };
  }

  /**
     * Called when the component has been instantiated. This is useful to define
     * default instance variable values.
     *
     * @param component - The JSON representation of the component created.
     * @param options - The global options for the renderer
     * @param data - The contextual data object (model) used for this component.
     */
  constructor(component, options, data) {
    super(component, options, data);
  }

  static get serverConditionSettings() {
    return BodyMapComponent.conditionOperatorsSettings;
  }

  static get conditionOperatorsSettings() {
    return {
      ...super.conditionOperatorsSettings,
      operators: ["isEmpty", "isNotEmpty"],
    };
  }

  // static savedValueTypes(schema) {
  //   schema = schema || {};
  //   return getComponentSavedTypes(schema) || [componentValueTypes.string];
  // }

  /**
   * Called immediately after the component has been instantiated to initialize
   * the component.
  */
  init() {
    super.init();
    this.currentWidth = 0;
    this.scale = 1;

    if (!this.component.width) {
      this.component.width = "300px";
    }
    if (!this.component.height) {
      this.component.height = "400px";
    }

    if (
      this.component.keepOverlayRatio
      && this.options?.display === "pdf"
      && this.component.overlay?.width
      && this.component.overlay?.height
    ) {
      this.ratio = this.component.overlay?.width / this.component.overlay?.height;
      this.component.width = "100%";
      this.component.height = "auto";
    }
  }

  get emptyValue() {
    return "";
  }

  get defaultSchema() {
    return BodyMapComponent.schema();
  }

  /**
   * For Input based components, this returns the <input> attributes that should
   * be added to the input elements of the component. This is useful if you wish
   * to alter the "name" and "class" attributes on the <input> elements created
   * within this component.
   *
   * @return - A JSON object that is the attribute information to be added to the
   *           input element of a component.
  */
  get inputInfo() {
    const info = super.inputInfo;
    info.type = "input";
    info.attr.type = "hidden";
    return info;
  }

  get className() {
    return `${super.className}`;
  }

  get labelPositions() {
    return this.labelPosition.split("-");
  }

  labelIsHidden() {
    return this.component.hideLabel;
  }

  /**
     * Sets the value of both the data and view of the component (such as setting the
     * <input> value to the correct value of the data. This is most commonly used
     * externally to set the value and also see that value show up in the view of the
     * component. If you wish to only set the data of the component, like when you are
     * responding to an HMTL input event, then updateValue should be used instead since
     * it only sets the data value of the component and not the view.
     *
     * @param value - The value that is being set for this component"s data and view.
     * @param flags - Change propogation flags that are being used to control behavior of the
     *                change proogation logic.
     *
     * @return - Boolean indicating if the setValue changed the value or not.
  */
  setValue(value, flags = {}) {
    const changed = super.setValue(value, flags);

    if (this.refs.signatureImage && (this.options.readOnly || this.disabled) && value) {
      this.refs.signatureImage.setAttribute("src", value);
      this.showCanvas(false);
    }

    if (this.signaturePad && changed) {
      this.triggerChange();
    }

    if (this.signaturePad && this.dataValue && this.signaturePad.isEmpty()) {
      this.setDataToSigaturePad();
    }

    return changed;
  }

  showCanvas(show) {
    if (show) {
      if (this.refs.canvas) {
        this.refs.canvas.style.display = "inherit";
      }
      if (this.refs.signatureImage) {
        this.refs.signatureImage.style.display = "none";
      }
    } else {
      if (this.refs.canvas) {
        this.refs.canvas.style.display = "none";
      }
      if (this.refs.signatureImage) {
        this.refs.signatureImage.style.display = "inherit";
        this.refs.signatureImage.style.maxHeight = "100%";
      }
    }
  }

  onDisabled() {
    this.showCanvas(!super.disabled);
    if (this.signaturePad) {
      if (super.disabled) {
        this.signaturePad.off();
        if (this.refs.refresh) {
          this.refs.refresh.classList.add("disabled");
        }
        if (this.refs.signatureImage && this.dataValue) {
          this.refs.signatureImage.setAttribute("src", this.dataValue);
        }
      } else {
        this.signaturePad.on();
        if (this.refs.refresh) {
          this.refs.refresh.classList.remove("disabled");
        }
      }
    }
  }

  checkSize(force = 0, scale = 0) {
    this.fixFlexStyle();

    if (this.refs.padBody || (force || this.refs.padBody && this.refs.padBody.offsetWidth !== this.currentWidth)) {
      this.scale = force ? scale : this.scale;
      this.currentWidth = this.refs.padBody.offsetWidth;
  
      // Calculate the aspect ratio if it's not provided
      if (!this.ratio && this.refs.canvas.width && this.refs.canvas.height) {
        this.ratio = this.refs.canvas.width / this.refs.canvas.height;
      }
  
      // Calculate the new width and height based on the aspect ratio and scale
      let width = this.currentWidth * this.scale;
      let height = width / this.ratio;
  
      // Check if the new height exceeds the container's height
      if (height > this.refs.padBody.offsetHeight * this.scale) {
        // Adjust width and height to fit within the container
        height = this.refs.padBody.offsetHeight * this.scale;
        width = height * this.ratio;
      }
  
      // Set the canvas dimensions while maintaining the aspect ratio
      this.refs.canvas.width = width;
      this.refs.canvas.height = height;
      this.refs.canvas.style.width = `${width}px`;
      this.refs.canvas.style.height = `${height}px`;
      const ctx = this.refs.canvas.getContext("2d");
      ctx.setTransform(1, 0, 0, 1, 0, 0);
      ctx.scale(this.scale, this.scale);
      ctx.fillStyle = this.signaturePad.backgroundColor;
      ctx.fillRect(0, 0, this.refs.canvas.width / this.scale, this.refs.canvas.height / this.scale);
  
      this.setDataToSigaturePad();
      this.showCanvas(true);
    }
  }

  fixFlexStyle() {
    const isRightAligned = this.labelPositions[0] === "right";
    
    // container of BodyMap
    if(!this.formioForm) return;

    // div created when labelPosition is "left" or "right"
    const fieldWrapper = document.getElementsByClassName("field-wrapper")[0];

    if(!fieldWrapper) return;
    
    // prevent BodyMap from going out of bounds of the container 
    if(this.formioForm?.clientWidth < 335) {
        (fieldWrapper as HTMLDivElement).style.flexDirection = "column";
    } else {
      (fieldWrapper as HTMLDivElement).style.flexDirection = isRightAligned ? "row-reverse" : "row";
    }
  }

  renderElement(value, index) {
    return this.renderTemplate("signature", {
      element: super.renderElement(value, index),
      required: _.get(this.component, "validate.required", false),
    });
  }

  get hasModalSaveButton() {
    return false;
  }

  getModalPreviewTemplate() {
    return this.renderTemplate("modalPreview", {
      previewText: this.dataValue ?
        `<img src=${this.dataValue} ref="openModal" style="width: 100%;height: 100%;" />` :
        this.t("Click to Sign")
    });
  }

  /**
   * The attach method is called after "render" which takes the rendered contents
   * from the render method (which are by this point already added to the DOM), and
   * then "attach" this component logic to that html. This is where you would load
   * any references within your templates (which use the "ref" attribute) to assign
   * them to the "this.refs" component variable (see comment below).
   *
   * @param - The parent DOM HtmlElement that contains the component template.
   *
   * @return - A Promise that will resolve when the component has completed the
   *           attach phase.
  */
  attach(element) {
    /**
     * This method will look for an element that has the "ref="customRef"" as an
     * attribute (like <div ref="customRef"></div>) and then assign that DOM
     * element to the variable "this.refs". After this method is executed, the
     * following will point to the DOM element of that reference.
     *
     * this.refs.customRef
     *
     * For DOM elements that have multiple in the component, you would make this
     * say "customRef: "multiple"" which would then turn "this.refs.customRef" into
     * an array of DOM elements.
    */
    this.loadRefs(element,
      {
        canvas: "single",
        refresh: "single",
        padBody: "single",
        signatureImage: "single"
      });
    const superAttach = super.attach(element);

    if (this.refs.refresh && this.options.readOnly) {
      this.refs.refresh.classList.add("disabled");
    }

    const formioForm = document.getElementsByClassName("formio-form");

    // get the container for the BodyMap component
    // when editing the component there will be many elements with "formio-form" class
    // when saved only will be 1, that container it's what we need
    if(formioForm && formioForm.length == 1) {
      this.formioForm = formioForm[0];
    }

    // Create the signature pad.
    if (this.refs.canvas) {
      this.signaturePad = new SignaturePad(this.refs.canvas, {
        minWidth: this.component.minWidth,
        maxWidth: this.component.maxWidth,
        penColor: this.component.penColor,
        backgroundColor: this.component.backgroundColor
      });

      this.signaturePad.addEventListener("endStroke", () => this.setValue(this.signaturePad.toDataURL()));

      this.onDisabled();

      // Ensure the signature is always the size of its container.
      if (this.refs.padBody) {
        if (!this.refs.padBody.style.maxWidth) {
          this.refs.padBody.style.maxWidth = "100%";
        }

        if (!this.builderMode && !this.options.preview) {
          this.observer = new ResizeObserver(() => {
            this.checkSize();
          });

          this.observer.observe(this.refs.padBody);
        }

        this.addEventListener(window, "resize", _.debounce(() => this.checkSize(), 10));

        setTimeout(function checkWidth() {
          if (this.refs.padBody && this.refs.padBody.offsetWidth) {
            this.checkSize();
          } else {
            setTimeout(checkWidth.bind(this), 20);
          }
        }.bind(this), 20);
      }
    }

    /**
     * It is common to attach events to your "references" within your template.
     * This can be done with the "addEventListener" method and send the template
     * reference to that object.
    */
    this.addEventListener(this.refs.refresh, "click", (event) => {
      event.preventDefault();
      this.showCanvas(true);
      this.clearSignaturePad();
    });
    return superAttach;
  }
  /* eslint-enable max-statements */

  /**
   * Called when the component has been detached. This is where you would destroy
   * any other instance variables to free up memory. Any event registered with
   * "addEventListener" will automatically be detached so no need to remove them
   * here.
   *
   * @return - A Promise that resolves when this component is done detaching.
  */
  detach() {
    if (this.observer) {
      this.observer.disconnect();
      this.observer = null;
    }

    if (this.signaturePad) {
      this.signaturePad.off();
    }
    this.signaturePad = null;
    this.currentWidth = 0;
    super.detach();
  }

  getValueAsString(value) {
    if (_.isUndefined(value) && this.inDataTable) {
      return "";
    }
    return value ? "Yes" : "No";
  }

  focus() {
    this.refs.padBody.focus();
  }

  setDataToSigaturePad() {
    this.signaturePad.clear();

    // property name for the component
    const propertyName = this.component.key;

    const imageToDraw = !!this._data[propertyName] ? this._data[propertyName] : bodyImageFrontAndBack;

    this.signaturePad.fromDataURL(imageToDraw, {
      ratio: 1,
      width: this.refs.canvas.width,
      height: this.refs.canvas.height,
    });
  }

  clearSignaturePad() {
    this.signaturePad.clear();

    this.signaturePad.fromDataURL(bodyImageFrontAndBack, {
      ratio: 1,
      width: this.refs.canvas.width,
      height: this.refs.canvas.height,
    });
  }

  /**
   * A very useful method that will take the values being passed into this component
   * and convert them into the "standard" or normalized value. For exmample, this
   * could be used to convert a string into a boolean, or even a Date type.
   *
   * @param value - The value that is being passed into the "setValueAt" method to normalize.
   * @param flags - Change propogation flags that are being used to control behavior of the
   *                change proogation logic.
   *
   * @return - The "normalized" value of this component.
  */
  normalizeValue(value, flags = {}) {
    return super.normalizeValue(value, flags);
  }

  /**
   * Called when the component has been completely "destroyed" or removed form the
   * renderer.
   *
   * @return - A Promise that resolves when this component is done being destroyed.
  */
  destroy() {
    return super.destroy();
  }

}

// testing

// Option1
// Formio.use({
//   components: {
//     bodymap: BodyMapComponent
//   }
// });

// Option2
Components.addComponent("bodymap", BodyMapComponent);