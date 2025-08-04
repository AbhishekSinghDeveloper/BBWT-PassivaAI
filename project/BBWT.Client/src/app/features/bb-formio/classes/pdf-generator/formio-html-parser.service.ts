import { Injectable } from "@angular/core";
import { FormioComponentSelectors } from "./formio-components-selectors";

import { FormioFormValuesParserService } from "./formio-form-values.service";

@Injectable({
    providedIn: "root"
})
export class FormioHtmlParserService {

    private parser = new DOMParser();
    private html: Document = null;

    constructor(private formValuesParser: FormioFormValuesParserService) {
    }

    public parse = (htmlString: string, formData?: any) => {
        this.html = this.parser.parseFromString(htmlString, "text/html");
        
        this.html = this.formValuesParser.applyFormValues(htmlString, formData);
        
        this.html = this.removeSubmitButton(this.html);
        
        this.html = this.removeTooltips(this.html);
        
        // tabs options
        // this.html = this.handleFormioTabComponentOnlyTabContents(this.html);
        this.html = this.handleFormioTabComponentOnePageByEveryTab(this.html);
        
        const fC: any = this.html.body.firstChild;
        
        (<HTMLDivElement> fC).className = (<HTMLDivElement> fC).className.replace("card bg-light", "");
        
        return this.html;
    }

    private removeSubmitButton = (html: Document): Document => {
        const formioComponentSubmit = "formio-component-submit";

        const submitButtons = html.getElementsByClassName(formioComponentSubmit);

        const divElement = document.createElement("div");

        for (let index = 0; index < submitButtons.length; index++) {
            const element = submitButtons[index];

            element.parentNode?.replaceChild(divElement, element);
        }

        return html;
    }

    private removeTooltips = (html: Document): Document => {
        const tooltips = html.querySelectorAll("[data-tooltip");

        const divElement = document.createElement("div");

        for (let index = 0; index < tooltips.length; index++) {
            const element = tooltips[index];

            element.parentNode?.replaceChild(divElement, element);
        }

        return html;
    }

    private handleFormioTabComponentOnlyTabContents = (html: Document) => {
        const tabsElement = html.getElementsByClassName(FormioComponentSelectors.Tabs);
        const newElement = document.createElement("div");

        // remove child with class = "card-header"
        for (let index = 0; index < tabsElement.length; index++) {
            // every tab-pane content has first child a <div class="card"
            const divClassCard = <HTMLElement>tabsElement[index].firstChild;
            // divClassCard.className = divClassCard?.className?.replace("card", "");

            for (let j = 0; j < divClassCard.childNodes.length; j++) {
                const element = <HTMLElement>divClassCard.childNodes[j];

                if (element.className.includes("card-header")) {
                    element.parentNode?.replaceChild(newElement, element);
                }
            }
        }

        this.handleTabContentsDisplayNoneStyle(html);

        return html;
    }

    private handleFormioTabComponentOnePageByEveryTab = (html: Document) => {
        const tabsElement = html.getElementsByClassName(FormioComponentSelectors.Tabs);

        let finalTabs = "";

        let tabHeader = null;
        const tabContents: Array<HTMLElement> = [];

        for (let index = 0; index < tabsElement.length; index++) {
            // every tab-pane content has first child a <div class="card"
            const divClassCard = <HTMLElement>tabsElement[index].children[0];
            // divClassCard.className = divClassCard?.className?.replace("card", "");

            let aux = 0;
            while (divClassCard.childElementCount > 0) {
                const element = <HTMLElement>divClassCard.children[0];

                if (element?.className?.includes("card-header")) {
                    tabHeader = element;
                } else {
                    tabContents.push(element);
                }

                divClassCard.removeChild(element);
                aux++;

            }

            for (let index = 0; index < tabContents.length; index++) {
                const tabContent = <HTMLElement>tabContents[index];
                tabContent.id = "tab-content-" + index;
                tabContent.style.display = "block";
                // add TabHeader active at index
                const tabHeaderToAdd = this.modifyTabHeader(tabHeader, index);
                tabHeaderToAdd.id = "tab-header-" + index;
                // add tabContent.firstChild
                const pageBreak = this.getDivForPageBreak();
                pageBreak.id = "page-break-" + index;
                // add page-break

                const newElement = document.createElement("div");
                divClassCard.id = "new-div-" + index;

                newElement.appendChild(tabHeaderToAdd);
                newElement.appendChild(tabContent);
                index + 1 != tabContents.length && newElement.appendChild(pageBreak);

                finalTabs += newElement.innerHTML;

            }
            divClassCard.innerHTML = finalTabs;

        }

        this.handleTabContentsDisplayNoneStyle(html);

        return html;
    }

    private modifyTabHeader = (tabHeader: HTMLElement, activeListItemIndex: number) => {
        // tabHeader.firstChild = "ul"
        const tabHeaderList = tabHeader.children[0].children;

        // add class "active" to li = liIndexActive
        // and the <a> inside the <li>
        // remove class "active" to others <li>
        for (let index = 0; index < tabHeaderList.length; index++) {
            const li = tabHeaderList[index];
            const liChildAnchorTag = <HTMLElement>li.children[0];

            if (index == activeListItemIndex) {
                if (!li.className.includes("active")) {
                    li.classList.add("active")
                }

                if (!liChildAnchorTag.className.includes("active")) {
                    liChildAnchorTag.classList.add("active");
                }
            } else {
                li.className = li.className.replace("active", "");
                liChildAnchorTag.className = liChildAnchorTag.className.replace("active", "");
            }

        }

        return tabHeader;
    }

    private getDivForPageBreak = () => {
        const divElement = document.createElement("div");
        divElement.classList.add("page-break");
        divElement.style.pageBreakBefore = "always";

        return divElement;
    }

    private handleTabContentsDisplayNoneStyle = (html: Document) => {
        const selector = "card-body tab-pane";
        const tabPanes = html.getElementsByClassName(selector);

        for (let index = 0; index < tabPanes.length; index++) {
            const element = <HTMLElement>tabPanes[index];

            if (element.style.display !== "block") {
                element.style.display = "block"
            }
        }
    }

    //#region Sanitize HTML => not used for now
    public sanitize = (rawHTML: string) => {
        // Use DomSanitizer to sanitize and parse HTML
        const htmlStringWithoutComments = this.removeCommentsFromHtml(rawHTML);
        const removeNONHTMLTags = this.removeNonHtmlTags(htmlStringWithoutComments);
        // const sanitizeHTML = this.sanitizer.sanitize(SecurityContext.HTML, rawHTML);
        // const sanitizeHTML1 = sanitizeHTML.replace(/&#10;/g, "");
        // console.log(sanitizeHTML);
    }

    private removeCommentsFromHtml(htmlString: string): string {
        // Regular expression to find HTML comments
        const commentRegex = /<!--[\s\S]*?-->/g;

        // Remove comments using regular expression
        const htmlWithoutComments = htmlString.replace(commentRegex, "");

        return htmlWithoutComments;
    }

    private removeNonHtmlTags = (htmlString: string): string => {
        // Create a new DOMParser
        const parser = new DOMParser();

        // Parse the HTML
        const doc = parser.parseFromString(htmlString, "text/html");

        // Start removing invalid nodes from the document root
        this.replaceInvalidNodes(doc.body);

        // Get the modified HTML
        const sanitizedHtml = doc.body.innerHTML;

        return sanitizedHtml;
    }

    // Function to check if a html tag is valid
    private isValidHtmlTag = (tagName: string): boolean => {
        const validHtmlTags = [
            "a", "abbr", "address", "area", "article", "aside", "audio", "b", "base", "bdi", "bdo", "blockquote",
            "body", "br", "button", "canvas", "caption", "cite", "code", "col", "colgroup", "data", "datalist",
            "dd", "del", "details", "dfn", "dialog", "div", "dl", "dt", "em", "embed", "fieldset", "figcaption",
            "figure", "footer", "form", "h1", "h2", "h3", "h4", "h5", "h6", "head", "header", "hgroup", "hr", "html",
            "i", "iframe", "img", "input", "ins", "kbd", "label", "legend", "li", "link", "main", "map", "mark", "meta",
            "meter", "nav", "noscript", "object", "ol", "optgroup", "option", "output", "p", "param", "picture", "pre",
            "progress", "q", "rp", "rt", "ruby", "s", "samp", "script", "section", "select", "small", "source", "span",
            "strong", "style", "sub", "summary", "sup", "svg", "table", "tbody", "td", "template", "textarea", "tfoot",
            "th", "thead", "time", "title", "tr", "track", "u", "ul", "var", "video", "wbr"
        ];

        return validHtmlTags.includes(tagName.toLowerCase());
    }

    // Function to remove invalid nodes
    private replaceInvalidNodes = (node: any) => {
        if (node.nodeType === Node.ELEMENT_NODE) {
            const tagName = (node as HTMLElement).tagName;

            if (!this.isValidHtmlTag(tagName)) {
                const divElement = document.createElement("div");
                divElement.innerHTML = node.innerHTML;

                node.parentNode?.replaceChild(divElement, node);
            } else {
            }
            // Recursively replace child nodes
            node.childNodes.forEach(element => this.replaceInvalidNodes(element));
        }
    }

    //#endregion

}