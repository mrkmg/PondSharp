import * as PIXI from "pixi.js";

const win: typeof window & {Blazor: any} = window as any;

export class PondRenderer {
    private element: HTMLDivElement;
    private application: PIXI.Application;
    private entities: Record<number, PIXI.Container> = {};
    private fps: PIXI.Text;
    private gridSize: number;
    private width: number;
    private height: number;
    
    public constructor(element: HTMLElement, width: number, height: number, gridSize: number) {
        this.element = element as HTMLDivElement;
        this.width = width;
        this.height = height;
        this.gridSize = gridSize;
        this.init();
    }
    
    public start = () => this.application.start();
    public stop = () => this.application.stop();
    
    public resize(width: number, height: number, gridSize: number) {
        this.width = width;
        this.height = height;
        this.gridSize = gridSize;
        this.gridSize = gridSize;
        this.init();
    }
    
    private init() {
        if (this.application) {
            console.debug("Removing PIXI instance");
            this.application.destroy(true, {children: true, texture: true, baseTexture: true});
            this.element.removeChild(this.element.children.item(0));
            this.application = null;
        }
        console.debug("Creating PIXI instance");
        // noinspection JSSuspiciousNameCombination
        this.application = new PIXI.Application({
            width: this.width * this.gridSize,
            height: this.height * this.gridSize
        });
        this.element.appendChild(this.application.view);
        this.application.stage.x = this.application.view.width / 2;
        this.application.stage.y = this.application.view.height / 2;
        this.application.ticker.add(() => this.onTick());

        this.fps = new PIXI.Text("FPS: ??", {fill: 0xAAFFAA, fontSize: 8});
        this.fps.x = -this.application.view.width/2;
        this.fps.y = -this.application.view.height/2;
        this.application.stage.addChild(this.fps);
    }
    
    public createEntity(id: number, x: number, y: number, color: number) {
        const graphic = new PIXI.Graphics();
        graphic.beginFill(color);
        graphic.drawRect(0, 0, this.gridSize, this.gridSize);
        graphic.position.x = x * this.gridSize;
        graphic.position.y = y * this.gridSize;
        graphic.endFill()
        this.entities[id] = graphic;
        this.application.stage.addChild(graphic);
    }
    
    public destroyEntity(id: number) {
        this.entities[id].destroy();
        this.entities[id] = null;
        delete this.entities[id];
    }
    
    public moveEntity(id: number, x: number, y: number) {
        const container = this.entities[id];
        container.position.x = x * this.gridSize;
        container.position.y = y * this.gridSize;
    }
    
    public changeEntityColor(id: number, color: number) {
        const graphic = this.entities[id] as PIXI.Graphics;
        graphic.clear();
        graphic.beginFill(color);
        graphic.drawRect(0, 0, this.gridSize, this.gridSize);
        graphic.endFill();
    }
    
    public processEntityChangeRequestsRaw(pointer: any) {        
        const length = win.Blazor.platform.getArrayLength(pointer);
        for (let i = 0; i < length; i++) {
            const entryPtr = win.Blazor.platform.getArrayEntryPtr(pointer, i, 16);
            const id = win.Blazor.platform.readInt32Field(entryPtr, 0);
            const x = win.Blazor.platform.readInt32Field(entryPtr, 4);
            const y = win.Blazor.platform.readInt32Field(entryPtr, 8);
            const color = win.Blazor.platform.readInt32Field(entryPtr, 12);
            if (x != -2147483648 && y != -2147483648)
                this.moveEntity(id, x, y);
            if (color != -2147483648)
                this.changeEntityColor(id, color);
        }
        return true;
    }
    
    private onTick() {
        this.fps.text = `FPS: ${Math.round(PIXI.Ticker.shared.FPS)}`;
    }
}
