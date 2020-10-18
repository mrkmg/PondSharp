import {PondRenderer} from "./pond/pond-renderer";
import {CodeEditor} from "./code/code-editor";

const win = window as any;
win.ponds = {};
win.lastPond = 0;
win.createPond = async function (element: HTMLElement, width: number, height: number, gridSize: number) {
    win.lastPond++;
    const id = `pond${win.lastPond}`
    win.ponds[id] = new PondRenderer(element, width, height, gridSize);
    return id;
};
win.createEditor = async function (elementId: string, globalName: string) {
    win[globalName] = new CodeEditor(elementId)
}