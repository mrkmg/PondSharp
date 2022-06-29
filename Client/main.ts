// import "./css/app.scss";
import {PondRenderer} from "./Pond/pond-renderer";
import {CodeEditor} from "./IDE/ide-element";

/// Defined by webpack.config.js
declare var self: any;

self.ponds = {};
self.lastPond = 0;
self.createPond = async function (pondRef: any, element: HTMLElement, width: number, height: number, gridSize: number) {
    self.lastPond++;
    const id = `pond_${self.lastPond}`;
    self.ponds[id] = new PondRenderer(pondRef, element, width, height, gridSize);
    return id;
};
self.createEditor = async function (elementId: string, globalName: string) {
    self[globalName] = new CodeEditor(elementId)
}
self.getSize = () => [self.innerWidth, self.innerHeight]
self.bindWindowResize = (obj: any) => {
    let windowResizeDebounce: NodeJS.Timeout;
    window.onresize = () => {
        clearTimeout(windowResizeDebounce);
        windowResizeDebounce = setTimeout(() => obj.invokeMethodAsync("OnWindowResized"), 1000);
    }
}