import {editor} from "monaco-editor";

export class CodeEditor {
    private editor: editor.IStandaloneCodeEditor;
    
    constructor(private elementId: string) {
        this.editor = editor.create(document.getElementById(elementId), {
            value: "",
            language: "csharp",
            theme: 'vs-dark',
        });
    }
    
    public setCode(code: string) {
        this.editor.setValue(code);
    }
    
    public getCode(): string {
        return this.editor.getValue();
    }
}