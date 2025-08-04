import { Injectable } from "@angular/core";
import * as moment from "moment";

import { bodyImageFrontAndBack } from "@features/bb-formio/components/custom-components/body-map/fixtures/bodymapFrontAndBack";
import { FormioComponentSelectors } from "./formio-components-selectors";

@Injectable({
    providedIn: "root"
})
export class FormioFormValuesParserService {

    private parser = new DOMParser();
    private html: Document = null;
    private formData: Object = null;

    private classesToRemove = [
        "formio-form-group",
        "form-group",
        "has-feedback",
        "formio-component",
        "signature-pad"
    ];

    constructor() {
    }

    public applyFormValues = (htmlString: string, formData: any) => {
        if (!formData) return;

        this.html = this.parser.parseFromString(htmlString, "text/html");

        // for testing
        this.formData = formData;

        this.applyFormValuesToHtml(this.formData, this.html);

        return this.html;
    }

    private applyFormValuesToHtml = (formioFormData: any, html: Document) => {
        const formioComponents = html.getElementsByClassName("formio-component");

        if (formioComponents.length == 0) return;

        while (formioComponents.length > 0) {
            const element = formioComponents[0];

            if (element) {
                const classValues = element?.classList;

                classValues?.remove(...this.classesToRemove);
                const formioComponent = classValues[0];
                const formioComponentIdClass = classValues[1];

                switch (formioComponent) {
                    case FormioComponentSelectors.TextArea:
                        this.handleFormioTextAreaComponent(element);
                        break;
                    case FormioComponentSelectors.SelectBoxes:
                        this.handleFormioSelectBoxesComponent(element, formioComponentIdClass);
                        break;
                    case FormioComponentSelectors.Select:
                        this.handleFormioSelectComponet(element, formioComponentIdClass);
                        break;
                    case FormioComponentSelectors.Radio:
                        this.handleFormioRadioComponet(element, formioComponentIdClass ?? formioComponent);
                        break;
                    case FormioComponentSelectors.Tags:
                        this.handleFormioTagsComponet(element, formioComponentIdClass ?? formioComponent);
                        break;
                    case FormioComponentSelectors.Address:
                        this.handleFormioAddressComponent(element, formioComponentIdClass ?? formioComponent);
                        break;
                    case FormioComponentSelectors.DateTime:
                        this.handleFormioDateTimeComponent(element, formioComponentIdClass ?? formioComponent);
                        break;
                    case FormioComponentSelectors.Day:
                        this.handleFormioDayComponent(element, formioComponentIdClass ?? formioComponent);
                        break;
                    case FormioComponentSelectors.Time:
                        this.handleFormioTimeComponent(element, formioComponentIdClass ?? formioComponent);
                        break;
                    // TODO: handle Currency component
                    case FormioComponentSelectors.Survey:
                        this.handleFormioSurveyComponent(element, formioComponentIdClass ?? formioComponent);
                        break;
                    case FormioComponentSelectors.Signature:
                        this.handleFormioSignatureComponent(element, formioComponentIdClass ?? formioComponent);
                        break;
                    case FormioComponentSelectors.BodyMap:
                        this.handleFormioBodyMapComponent(element, formioComponentIdClass ?? formioComponent);
                        break
                    default:
                        this.handleFormioInputTags(element);
                        break;
                }
            }
        }
    }

    // TODO: Refactor/Rework - TEST other cases - multiples selects in the same form - 
    private handleFormioSelectComponet = (component: any, classWithId: string) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");
            const property = classWithId.split("-").pop();
            const formValue = this.formData[property];

            // Formio Select Component Options Widgets = ChoiceJS | HTML5
            // ChoiceJS Widget option
            const choice = _componentAsDocument.querySelectorAll(`[data-value='${formValue}']`);

            if (choice.length == 0) {
                // HTML5 Widget option
                const selectedOption = _componentAsDocument.querySelectorAll(`[value='${formValue}']`);

                if (selectedOption.length == 0) return;

                (selectedOption[0] as HTMLOptionElement).setAttribute("selected", "true");

                component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);

                return;
            }

            // this div will have one children = the selected choice
            const addChoiceToThisElement = _componentAsDocument.getElementsByClassName("choices__list choices__list--single");
            addChoiceToThisElement[0].appendChild(choice[0]);

            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);
        } catch (error) {
            console.log({ error });
        }
    }

    // TODO: Refactor/Rework
    private handleFormioSelectBoxesComponent = (component: any, classWithId: string) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");

            const inputsCollection = _componentAsDocument.getElementsByTagName("input");
            const property = classWithId.split("-").pop();

            if (inputsCollection.length == 0) return;

            for (let i = 0; i < inputsCollection.length; i++) {
                const input = inputsCollection[i];

                const checkboxPropertyId = input.id.split("-").pop();
                const formValue = this.formData[property][checkboxPropertyId];

                if (formValue) {
                    input.setAttribute("checked", "true");
                }
            }

            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);

        } catch (error) {
            console.log({ error });
        }
    }

    private handleFormioInputTags = (component: any) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");

            const inputsCollection = _componentAsDocument.getElementsByTagName("input");

            if (inputsCollection.length == 0) return;

            const inputElement = inputsCollection[0];

            const formDataId = inputElement.id != "" ? inputElement.id.split("-").pop() : this.getPropertyNameFromInputDataAtrb(inputElement.name);
            const keys = formDataId.split(".");      

            let formValue = this.formData[keys[0]];
            let index = 1;
            while (index < keys.length){
                formValue = formValue[keys[index]];
                index++;
            }

            // prevent inputs with value="[object Object]"
            // when a component reach this point and the formValue is an object
            // that component have to be handled in another method
            if(!formValue || typeof formValue === "object") return;

            switch (inputElement.type) {
                case "text":
                    formValue && (inputElement as HTMLInputElement).setAttribute("value", formValue);
                    break;
                case "email":
                    formValue && (inputElement as HTMLInputElement).setAttribute("value", formValue);
                    break;
                case "checkbox":
                    formValue && inputElement.setAttribute("checked", "true");
                    break;
            }

            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);

        } catch (error) {
            console.log({ error });
        }
    }

    private handleFormioTextAreaComponent = (component: any) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");

            const inputsCollection = _componentAsDocument.getElementsByTagName("textarea");

            if (inputsCollection.length == 0) return;

            const textAreaElement = inputsCollection[0];

            const formDataId = textAreaElement.id.split("-")[1];
            const formValue = this.formData[formDataId];

            (textAreaElement as HTMLTextAreaElement).innerHTML = formValue ?? "";

            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);

        } catch (error) {
            console.log({ error });
        }
    }

    // TODO: Refactor/Rework - TEST other cases - multiples selects in the same form - 
    private handleFormioRadioComponet = (component: any, classWithId: string) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");

            const inputsCollection = _componentAsDocument.getElementsByTagName("input");
            const property = classWithId.split("-").pop();

            if (inputsCollection.length == 0) return;

            for (let i = 0; i < inputsCollection.length; i++) {
                const input = inputsCollection[i];

                const formDataId = input.id.split("-").pop();
                const formValue = this.formData[property];

                if (formDataId === formValue) {
                    input.setAttribute("checked", "true");
                }
            }

            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);

        } catch (error) {
            console.log({ error });
        }
    }

    // TODO: Refactor/Rework - TEST other cases - multiples selects in the same form - 
    private handleFormioTagsComponet = (component: any, classWithId: string) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");
            const property = classWithId.split("-").pop();

            // TODO: look for separator of tags on JSON generated for Formio
            const delimiter = ",";
            // option 1 => ","
            const formValue = this.formData[property];
            const tags: string[] = formValue.split(delimiter);

            if (tags.length === 0) return;

            const addTagsToThisElement = _componentAsDocument.getElementsByClassName("choices__list choices__list--multiple")[0];

            tags.forEach(tag => {
                const tagDiv = _componentAsDocument.createElement("div");
                tagDiv.className = "choices__item choices__item--selectable";
                tagDiv.innerText = tag;

                addTagsToThisElement.appendChild(tagDiv);
            });

            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);
        } catch (error) {
            console.log({ error });
        }
    }

    private handleFormioAddressComponent = (component: Element, classWithId: string) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");

            const inputsCollection = _componentAsDocument.getElementsByTagName("input");
            const property = classWithId.split("-").pop();
            
            if (inputsCollection.length == 0) return;

            // the Address component has two ways of display an address
            // 1. several inputs for the user to fill(address1, address2, city, state, country, zip code)
            // 2. an address from an address provider like GoogleMaps
            if((<Object>this.formData[property])?.hasOwnProperty("mode")) {
                const manualAddress = this.formData[property]?.address;

                for (let i = 0; i < inputsCollection.length; i++) {
                    const input = inputsCollection[i];
                    
                    const inputId = input?.id?.split("-")?.pop();
                    const formValue = manualAddress[inputId];
                    
                    input.setAttribute("value", formValue ?? "");
                }
            } else {

                // 2. an address from an address provider like GoogleMaps
                // normal Address component
                // just an input with an address
                for (let i = 0; i < inputsCollection.length; i++) {
                    const input = inputsCollection[i];

                    // the property "display_name" of the object "address" 
                    // will have the full address from the address provider
                    const formValue = this.formData[property]?.display_name;
    
                    input.setAttribute("value", formValue ?? "");
                }
            }
            
            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);

        } catch (error) {
            console.log({ error });
        }
    }

    /////////////
    // TODO: Refactor/Rework
    private handleFormioDateTimeComponent = (component: Element, classWithId: string) => {
        try {
            let _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");

            const inputsCollection = _componentAsDocument.getElementsByTagName("input");
            const property = classWithId.split("-").pop();

            // TODO: get format from DateTime component JSON
            const dateFormat = "yyyy-MM-DD hh:mm a";

            if (inputsCollection.length == 0) return;

            for (let i = 0; i < inputsCollection.length; i++) {
                const input = inputsCollection[i];

                const formValue = this.formData[property];

                const date = moment(formValue).format(dateFormat);

                input.setAttribute("value", date ?? "");
            }
            _componentAsDocument = this.removeIconFromDateTimeComponent(_componentAsDocument);
            
            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);
        } catch (error) {
            console.log({ error });
        }
    }

    private removeIconFromDateTimeComponent = (html: Document): Document => {
        const classSelector = "input-group-append";

        const elements = html.getElementsByClassName(classSelector);

        const divElement = document.createElement("div");

        for (let index = 0; index < elements.length; index++) {
            const element = elements[index];
            element.parentNode?.replaceChild(divElement, element);
        }

        return html;
    }
    /////////////

    // TODO: Refactor/Rework
    private handleFormioDayComponent = (component: any, classWithId: string) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");

            // 1 select for Month
            // 2 inputs => day and year
            // input day will have id="${formProperty}-day"
            // input year will have id="${formProperty}-year"
            const property = classWithId.split("-").pop();

            const formValue: string = this.formData[property];

            // TODO: the element for year/day/month can be an Input or a Select elements, need to check
            const inputForYear = _componentAsDocument.getElementById(`${property}-year`);
            const inputForDay = _componentAsDocument.getElementById(`${property}-day`);
            const typeOfInputForMonth = _componentAsDocument.getElementById(`${property}-month`)

            const valueAsDate = new Date(formValue);

            const year = moment(valueAsDate, true).format("yyyy");
            const day = moment(valueAsDate, true).format("DD");
            const month = moment(valueAsDate, true).format("MM");

            inputForDay.setAttribute("value", day ?? "");
            inputForYear.setAttribute("value", year.toString() ?? "");

            if (typeOfInputForMonth.tagName === "SELECT") {
                (typeOfInputForMonth as HTMLSelectElement).options.item(+month).setAttribute("selected", "true");
            } else {
                // input
                typeOfInputForMonth.setAttribute("value", month ?? "");
            }

            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);

        } catch (error) {
            console.log({ error });
        }
    }

    // TODO: Refactor/Rework
    private handleFormioTimeComponent = (component: any, classWithId: string) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");

            const inputsCollection = _componentAsDocument.getElementsByTagName("input");
            const property = classWithId.split("-").pop();

            if (inputsCollection.length == 0) return;

            for (let i = 0; i < inputsCollection.length; i++) {
                const input = inputsCollection[i];

                const formValue = this.formData[property];

                input.setAttribute("value", formValue ?? "");
            }

            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);

        } catch (error) {
            console.log({ error });
        }
    }

    private handleFormioSurveyComponent = (component: any, classWithId: string) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");
            const property = classWithId.split("-").pop();
            const formValue = this.formData[property];


            // The Survey component is made up of a series of questions and answers,
            // Each question has a series of possible answers.
            // Each answer will be an input="radio"
            // The data for this component is composed in this way
            // {
            //      "question1" : "selectedAnswerForQuestion1"
            //      "question2" : "selectedAnswerForQuestion2"
            //      ...
            // }
            // Every answer input="radio" will have an id = componentPropertyName + '-' + questionPropertyName + "-" + questionPropertyValue

            Object.keys(formValue).forEach(questionPropertyName => {
                const _input: any = _componentAsDocument.getElementById(`${property}-${questionPropertyName}-${formValue[questionPropertyName]}`);

                if ((_input as HTMLInputElement)?.type === "radio") {
                    (_input as HTMLInputElement).setAttribute("checked", "true");
                }
            });

            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);
        } catch (error) {
            console.log({ error });
        }
    }

    private handleFormioSignatureComponent = (component: any, classWithId: string) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");

            const formDataId = classWithId.split("-").pop().split(".");

            let formValue = this.formData[formDataId[0]]
            for(let i = 1; i < formDataId.length; i++) {
                formValue= formValue[formDataId[i]];
            }            

            const element = _componentAsDocument.querySelector(".signature-pad-footer");
            if (element) {
                element.parentNode.removeChild(element);
            }

            const _img = _componentAsDocument.getElementsByTagName("img")[0];
            const _canvas = _componentAsDocument.getElementsByTagName("canvas")[0];

            if (!_img || !_canvas) return;

            _img.style.display = "inherit";
            _img.setAttribute("src", formValue);
            _img.style.height = "100%";
            _img.style.maxWidth = "420px";
            _canvas.style.display = "none";

            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);

        } catch (error) {
            console.log({ error });
        }
    }

    private handleFormioBodyMapComponent = (component: Element, classWithId: string) => {
        try {
            const _componentAsDocument = this.parser.parseFromString(component.outerHTML, "text/html");

            const formDataId = classWithId.split("-").pop();;

            let propertyValue = this.recursivelyGetPropertyValue(this.formData, formDataId);

            if(propertyValue == "" || !propertyValue) {
                propertyValue = bodyImageFrontAndBack;
            }

            const _img = _componentAsDocument.getElementsByTagName("img")[0];
            const _canvas = _componentAsDocument.getElementsByTagName("canvas")[0];

            if (!_img || !_canvas) return;

            _img.style.display = "inherit";
            _img.setAttribute("src", propertyValue);
            _img.style.height = "100%";
            _canvas.style.display = "none";

            component.parentElement.replaceChild(_componentAsDocument.body.children[0], component);

        } catch (error) {
            console.log({ error });
        }
    }

    private recursivelyGetPropertyValue = (obj: object, propertyName: string) : string => {
        let result = "";

        for(const [key, value] of Object.entries(obj)) {
            if(key === propertyName) {
                result += value;
            }

            if(typeof value === "object") {
                result += this.recursivelyGetPropertyValue(value, propertyName);
            }
        }
        
        return result;
    }


    private getPropertyNameFromInputDataAtrb = (name: string) => {
        const pattern = /\[(.*?)\]/g;

        const match = name.match(pattern);

        if (!match) return "";

        // if name = "data[propertyName]" => value = "[propertyName]""
        const value = match.pop();

        // replace "[" and "]" with empty char
        return value.replace(/\[|\]/g, "");
    }

}
