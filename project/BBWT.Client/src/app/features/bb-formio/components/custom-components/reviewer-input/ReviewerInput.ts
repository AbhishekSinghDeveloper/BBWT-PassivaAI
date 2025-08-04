import { Components } from "formiojs";
import { FormioUtils } from "@formio/angular";

import { conformToMask } from "@formio/vanilla-text-mask";
import _ from "lodash";

import editForm from "./ReviewerInput.form";

const InputComponent = Components.components.input;

type LinkedTabValue = { linkedTab: string, value: string };

export default class ReviewerInputComponent extends (InputComponent as any) {
  public static editForm = editForm;

  static schema() {
    return InputComponent.schema({
      label: "Text Field",
      key: "reviewerInput",
      type: "reviewerInput",
      mask: false,
      inputType: "text",
      inputFormat: "plain",
      inputMask: "",
      displayMask: "",
      tableView: true,
      spellcheck: true,
      truncateMultipleSpaces: false
    });
  }

  static get builderInfo() {
    return {
      title: "Reviewer Input",
      icon: "terminal",
      group: "basic",
      documentation: "/userguide/form-building/form-components#text-field",
      weight: 0,
      schema: ReviewerInputComponent.schema()
    };
  }

  static get serverConditionSettings() {
    return ReviewerInputComponent.conditionOperatorsSettings;
  }

  static get conditionOperatorsSettings() {
    return {
      ...super.conditionOperatorsSettings,
      operators: [...super.conditionOperatorsSettings.operators, "includes", "notIncludes", "endsWith", "startsWith"],
      valueComponent(classComp) {
        return {
          ...classComp,
          type: "textfield",
        };
      }
    };
  }

  static savedValueTypes(schema) {
    return FormioUtils.getComponentSavedTypes(schema) || [FormioUtils.componentValueTypes.string];
  }

  get defaultSchema() {
    return ReviewerInputComponent.schema();
  }

  get inputInfo() {
    const info = super.inputInfo;
    info.type = "input";

    if (this.component.hasOwnProperty("spellcheck")) {
      info.attr.spellcheck = this.component.spellcheck;
    }

    if (this.component.mask) {
      info.attr.type = "password";
    } else {
      info.attr.type = (this.component.inputType === "password") ? "password" : "text";
    }
    info.changeEvent = (this.component.applyMaskOn === "blur") ? "blur" : "input";
    return info;
  }

  get emptyValue() {
    return "";
  }

  constructor(component, options, data) {
    super(component, options, data);

    const timezone = (this.component.widget?.timezone || this.options.timezone);
    const displayInTimezone = (this.component.widget?.displayInTimezone || "viewer");

    if (this.component.widget?.type === "calendar") {
      this.component.widget = {
        ...this.component.widget,
        readOnly: this.options.readOnly,
        timezone,
        displayInTimezone,
        locale: this.component.widget.locale || this.options.language,
        saveAs: "text"
      };
    }
  }

  attach(element) {
    this.loadRefs(element, {
      valueMaskInput: "single",
    });
    return super.attach(element);
  }

  /**
   * Returns the mask value object.
   *
   * @param value
   * @param flags
   * @return {*}
   */
  maskValue(value, flags = {} as any) {
    // Convert it into the correct format.
    if (!value || (typeof value !== "object")) {
      value = {
        value,
        maskName: this.component.inputMasks[0].label
      };
    }

    // If no value is provided, then set the defaultValue.
    if (!value.value) {
      const defaultValue = flags.noDefault ? this.emptyValue : this.defaultValue;
      value.value = Array.isArray(defaultValue) ? defaultValue[0] : defaultValue;
    }

    return value;
  }

  /**
   * Normalize the value set in the data object.
   *
   * @param value
   * @param flags
   * @return {*}
   */
  normalizeValue(value, flags = {} as any) {
    if (flags.fromSubmission) {
      // we are modifying the value in beforeSubmit and sending an object
      // when rendering data the value will be an object
      // return the value inside 
      if (typeof (value) === "object") {
        return super.normalizeValue((value as LinkedTabValue).value);
      }
    }

    if (!this.isMultipleMasksField) {
      return super.normalizeValue(value);
    }
    if (Array.isArray(value)) {
      return super.normalizeValue(value.map((val) => this.maskValue(val, flags)));
    }

    return super.normalizeValue(this.maskValue(value, flags));
  }

  /**
   * Sets the value at this index.
   *
   * @param index
   * @param value
   * @param flags
   */
  setValueAt(index, value, flags = {}) {
    if (!this.isMultipleMasksField) {
      return super.setValueAt(index, value.value ?? value, flags);
    }
    value = this.maskValue(value, flags);
    const textValue = value.value || "";
    const textInput = this.refs.mask ? this.refs.mask[index] : null;
    const maskInput = this.refs.select ? this.refs.select[index] : null;
    const mask = this.getMaskPattern(value.maskName);
    if (textInput && maskInput && mask) {
      if (textInput.inputmask) {
        this.setInputMask(textInput, mask);
        textInput.inputmask.setValue(textValue);
      } else {
        const placeholderChar = this.placeholderChar;
        textInput.value = conformToMask(textValue, FormioUtils.getInputMask(mask), { placeholderChar }).conformedValue;
      }
      maskInput.value = value.maskName;
    } else {
      return super.setValueAt(index, textValue, flags);
    }
  }

  /**
   * Returns the value at this index.
   *
   * @param index
   * @return {*}
   */
  getValueAt(index) {
    if (!this.isMultipleMasksField) {
      const value = super.getValueAt(index);
      const valueMask = this.component.inputMask;
      const displayMask = this.component.displayMask;

      // If the input has only the valueMask or the displayMask is the same as the valueMask,
      // just return the value which is already formatted
      if (valueMask && !displayMask || displayMask === valueMask) {
        return value;
      }

      // If there is only the displayMask, return the raw (unmasked) value
      if (displayMask && !valueMask) {
        return this.unmaskValue(value, displayMask);
      }

      return value;
    }
    const textInput = this.refs.mask ? this.refs.mask[index] : null;
    const maskInput = this.refs.select ? this.refs.select[index] : null;
    return {
      value: textInput ? textInput.value : undefined,
      maskName: maskInput ? maskInput.value : undefined
    };
  }

  isHtmlRenderMode() {
    return super.isHtmlRenderMode() ||
      ((this.options.readOnly || this.disabled) &&
        this.component.inputFormat === "html" &&
        this.type === "textfield");
  }

  isEmpty(value = this.dataValue) {
    if (!this.isMultipleMasksField) {
      return super.isEmpty((value || "").toString().trim());
    }
    return super.isEmpty(value) || (this.component.multiple ? value.length === 0 : (!value.maskName || !value.value));
  }

  truncateMultipleSpaces(value) {
    if (value) {
      return value.trim().replace(/\s{2,}/g, " ");
    }
    return value;
  }

  get validationValue() {
    const value = super.validationValue;
    if (value && this.component.truncateMultipleSpaces) {
      return this.truncateMultipleSpaces(value);
    }
    return value;
  }

  beforeSubmit() {
    let value = this.dataValue;

    const target: { label: string, key: string } = this.component?.targetTab;

    if (typeof (value) === "object") {
      value = (value as LinkedTabValue).value;
    }
    const newValue: LinkedTabValue = {
      linkedTab: target.key ?? null,
      value: value,
    }
    value = JSON.stringify(newValue);
    // update value
    this.dataValue = newValue;

    return Promise.resolve(value).then(() => super.beforeSubmit(value, () => { }));
  }

  getValueAsString(value, options) {
    if (options?.email && this.visible && !this.skipInEmail && _.isObject(value)) {
      const result = (`
        <table border="1" style="width:100%">
          <tbody>
          <tr>
            <th style="padding: 5px 10px;">${value.maskName}</th>
            <td style="width:100%;padding:5px 10px;">${value.value}</td>
          </tr>
          </tbody>
        </table>
      `);

      return result;
    }

    if (value && this.component.inputFormat === "plain" && /<[^<>]+>/g.test(value)) {
      value = value.replaceAll("<", "&lt;").replaceAll(">", "&gt;");
    }
    return super.getValueAsString(value, options);
  }
}

Components.addComponent("reviewerInput", ReviewerInputComponent);