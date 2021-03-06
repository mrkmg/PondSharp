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

    private onTick() {
        this.fps.text = `${Math.round(PIXI.Ticker.shared.FPS)} fps`;
    }

    start = () => this.application.start();
    stop = () => this.application.stop();

    resize(width: number, height: number, gridSize: number) {
        this.width = width;
        this.height = height;
        this.gridSize = gridSize;
        this.gridSize = gridSize;
        this.init();
    }
    
    createEntity(id: number, x: number, y: number, color: number) {
        const graphic = new PIXI.Graphics();
        graphic.beginFill(color);
        graphic.drawCircle(this.gridSize/2, this.gridSize/2, this.gridSize/2);
        graphic.position.x = x * this.gridSize;
        graphic.position.y = y * this.gridSize;
        graphic.endFill();
        this.entities[id] = graphic;
        this.application.stage.addChild(graphic);
    }
    
    destroyEntity(id: number) {
        if (!this.entities[id]) return;
        this.entities[id].destroy();
        this.entities[id] = null;
        delete this.entities[id];
    }
    
    moveEntity(id: number, x: number, y: number) {
        if (!this.entities[id]) return;
        const container = this.entities[id];
        container.position.x = x * this.gridSize;
        container.position.y = y * this.gridSize;
    }
    
    changeEntityColor(id: number, color: number) {
        if (!this.entities[id]) return;
        const graphic = this.entities[id] as PIXI.Graphics;
        graphic.clear();
        graphic.beginFill(color);
        graphic.drawCircle(this.gridSize/2, this.gridSize/2, this.gridSize/2);
        graphic.endFill();
    }
    
    processEntityChangeRequestsRaw(pointer: any) {        
        const length = win.Blazor.platform.getArrayLength(pointer);
        for (let i = 0; i < length; i++) {
            const entryPtr = win.Blazor.platform.getArrayEntryPtr(pointer, i, 16);
            const id = win.Blazor.platform.readInt32Field(entryPtr, 0);
            if (!this.entities[id]) continue;
            const type = win.Blazor.platform.readInt32Field(entryPtr, 4);
            switch (type) {
                case 1:
                    const x = win.Blazor.platform.readInt32Field(entryPtr, 8);
                    const y = win.Blazor.platform.readInt32Field(entryPtr, 12);
                    this.moveEntity(id, x, y);
                    break;
                case 2:
                    const color = win.Blazor.platform.readInt32Field(entryPtr, 8);
                    this.changeEntityColor(id, color);
                    break;
                case 0: // None, no more updates in this memory
                    return true;
                default:
                    throw new Error("unknown type");
            }
        }
        return true;
    }
}
