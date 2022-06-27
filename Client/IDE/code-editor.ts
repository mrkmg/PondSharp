import {editor} from "monaco-editor";

// @ts-ignore
self.MonacoEnvironment = {
    getWorkerUrl: function (moduleId: any, label: any) {
        if (label === 'json') {
            return './assets/json.worker.js';
        }
        if (label === 'css' || label === 'scss' || label === 'less') {
            return './assets/css.worker.js';
        }
        if (label === 'html' || label === 'handlebars' || label === 'razor') {
            return './assets/html.worker.js';
        }
        if (label === 'typescript' || label === 'javascript') {
            return './assets/ts.worker.js';
        }
        return './assets/editor.worker.js';
    }
};

export class CodeEditor {
    private editor: editor.IStandaloneCodeEditor;
    
    constructor(private elementId: string) {
        this.editor = editor.create(document.getElementById(elementId), {
            value: "",
            language: "csharp",
            theme: "vs-dark",
            automaticLayout: true,
        });
    }
    
    setCode(code: string) {
        this.editor.setValue(code);
    }
    
    getCode(): string {
        return this.editor.getValue();
    }
}