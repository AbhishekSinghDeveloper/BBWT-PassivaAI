import { HeadLoader, ScriptData } from "@bbwt/utils";

export class ZxcvbnLoad {
    static load() {
        return HeadLoader.loadScript(
            <ScriptData>{
                id: "zxcvbnscript",
                src: "https://cdnjs.cloudflare.com/ajax/libs/zxcvbn/4.4.2/zxcvbn.js",
                variable: "zxcvbn",
                integrityHash:
                    "sha384-jhGcGHNZytnBnH1wbEM3KxJYyRDy9Q0QLKjE65xk+aMqXFCdvFuYIjzMWAAWBBtR",
                crossOrigin: "anonymous"
            },
            true
        );
    }
}
