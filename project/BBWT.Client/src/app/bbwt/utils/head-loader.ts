export class HeadLoader {
    private static head = document.getElementsByTagName("head")[0];

    static loadScript(scriptData: ScriptData, async = false): Promise<any> {
        return new Promise((resolve, reject) => {
            let script = <HTMLScriptElement>(
                HeadLoader.head.querySelector(`script#${scriptData.id}`)
            );

            if (script) {
                if (script.src === scriptData.src) {
                    return resolve(scriptData.variable ? window[scriptData.variable] : null);
                } else {
                    this.removeScript(scriptData.id);
                }
            }

            script = document.createElement("script");
            script.id = scriptData.id;
            script.type = "text/javascript";
            script.src = scriptData.src;
            script.async = async;
            if (scriptData.integrityHash) {
                script.integrity = scriptData.integrityHash;
            }
            if (scriptData.crossOrigin || scriptData.crossOrigin === "") {
                script.crossOrigin = scriptData.crossOrigin;
            }

            script.onload = () => {
                resolve(scriptData.variable ? window[scriptData.variable] : null);
            };
            script.onerror = () => {
                reject();
            };
            HeadLoader.head.appendChild(script);
        });
    }

    static removeScript(id: string): void {
        const script = <HTMLScriptElement>HeadLoader.head.querySelector(`script#${id}`);
        if (script) script.remove();
    }

    static loadStyles(stylesData: StylesData): void {
        let stylesLink = <HTMLLinkElement>HeadLoader.head.querySelector(`link#${stylesData.id}`);
        if (stylesLink) {
            HeadLoader.head.removeChild(stylesLink);
        }

        stylesLink = document.createElement("link");
        stylesLink.id = stylesData.id;
        stylesLink.href = stylesData.href;
        stylesLink.rel = "stylesheet";
        stylesLink.type = "text/css";
        HeadLoader.head.appendChild(stylesLink);

        // Set a splash screen until the stylesheet file loading finish
        if (stylesData.splashScreenColor) {
            const splashScreen = document.createElement("div");
            splashScreen.style.position = "fixed";
            splashScreen.style.top = "0";
            splashScreen.style.left = "0";
            splashScreen.style.width = "100%";
            splashScreen.style.height = "100%";
            splashScreen.style.zIndex = "1000000000";
            splashScreen.style.backgroundColor = stylesData.splashScreenColor;
            document.body.appendChild(splashScreen);

            const dummyImg = document.createElement("img");
            dummyImg.onerror = () => {
                document.body.removeChild(dummyImg);
                document.body.removeChild(splashScreen);
            };
            document.body.appendChild(dummyImg);
            dummyImg.src = stylesData.href;
        }
    }

    static removeStyles(id: string) {
        const styles = <HTMLLinkElement>HeadLoader.head.querySelector(`link#${id}`);
        // Set empty href value to preserve position in head tag
        if (styles) {
            styles.href = "";
        }
    }
}

export type CrossOrigin = "anonymous" | "use-credentials" | "";

export class ScriptData {
    id: string;
    src: string;
    variable?: string;
    integrityHash?: string;
    crossOrigin?: CrossOrigin;
}

export class StylesData {
    id: string;
    href: string;
    splashScreenColor?: string;
}
