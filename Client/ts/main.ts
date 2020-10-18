import {PondRenderer} from "./pond-renderer";
import {CodeEditor} from "./code-editor";

const win = window as any;
win.ponds = {};
win.lastPond = 0;
win.createPond = function (element: HTMLElement, width: number, height: number, gridSize: number) {
    win.lastPond++;
    const id = `pond${win.lastPond}`
    win.ponds[id] = new PondRenderer(element, width, height, gridSize);
    return id;
};
win.createEditor = function (elementId: string, globalName: string) {
    win[globalName] = new CodeEditor(elementId)
}