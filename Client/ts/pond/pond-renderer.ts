import * as PIXI from "pixi.js";

const win: typeof window & {Blazor: any} = window as any;

export class PondRenderer {
    private pondRef: any;
    private element: HTMLDivElement;
    private application: PIXI.Application;
    private entities: Record<number, PIXI.Container> = {};
    private entityHolder: PIXI.Container;
    private fps: PIXI.Text;
    private cursor: PIXI.Graphics;
    private gridSize: number;
    private width: number;
    private height: number;
    private isMouseDown = false;
    private lastMouseLocation: {x: number, y: number};
    
    public constructor(pondRef: any, element: HTMLElement, width: number, height: number, gridSize: number) {
        this.pondRef = pondRef;
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
            height: this.height * this.gridSize,
        });
        this.application.renderer.plugins.interaction.cursorStyles.default = "none";
        this.element.appendChild(this.application.view);
        this.application.stage.x = this.application.view.width / 2;
        this.application.stage.y = this.application.view.height / 2;
        this.application.stage.width = this.application.view.width;
        this.application.stage.height = this.application.view.height;
        this.application.ticker.add(() => this.onTick());
        this.entityHolder = new PIXI.Container();
        this.application.stage.addChild(this.entityHolder);

        this.fps = new PIXI.Text("FPS: ??", {fill: 0xAAFFAA, fontSize: 8});
        this.fps.x = -this.application.view.width/2;
        this.fps.y = -this.application.view.height/2;
        this.application.stage.addChild(this.fps);
        
        this.cursor = new PIXI.Graphics();
        this.cursor.beginFill(0xFFFFFF);
        this.cursor.drawCircle(this.gridSize/2, this.gridSize/2, this.gridSize/2);
        this.cursor.endFill();
        this.cursor.position.x = 0;
        this.cursor.position.y = 0;
        
        this.application.stage.addChild(this.cursor);
        this.application.stage.interactive = true;
        
        this.application.view.addEventListener("mousemove", e => this.onMouseMove(e));
        this.application.view.addEventListener("mousedown", e => this.onMouseDown(e));
        this.application.view.addEventListener("mouseup", e => this.onMouseUp(e));
    }
    
    private onMouseMove(e: MouseEvent) {
        const gridLocation = this.getGridPosition(e);
        this.cursor.position.x = gridLocation.x * this.gridSize;
        this.cursor.position.y = gridLocation.y * this.gridSize;
        if (this.isMouseDown && (this.lastMouseLocation.x !== gridLocation.x || this.lastMouseLocation.y !== gridLocation.y)) {
            this.onClick(gridLocation.x, gridLocation.y);
        }
        this.lastMouseLocation = gridLocation;
    }
    
    private onMouseDown(e:  MouseEvent) {
        e.stopPropagation();
        this.isMouseDown = true;
        const gridLocation = this.getGridPosition(e);
        this.lastMouseLocation = gridLocation;
        this.onClick(gridLocation.x, gridLocation.y);
    }
    
    private onMouseUp(e:  MouseEvent) {
        e.stopPropagation();
        this.isMouseDown = false;
    }
    
    private onClick(x: number, y: number) {
        this.pondRef.invokeMethodAsync("OnClick", x, y);
    }
    
    private getGridPosition(e: MouseEvent): {x: number, y: number}
    {
        const stageXRaw = e.offsetX - this.application.stage.x;
        const stageYRaw = e.offsetY - this.application.stage.y;
        return {
            x: Math.round(stageXRaw/this.gridSize),
            y: Math.round(stageYRaw/this.gridSize)
        }
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
        graphic.endFill();
        graphic.position.x = x * this.gridSize;
        graphic.position.y = y * this.gridSize;
        this.entities[id] = graphic;
        this.entityHolder.addChild(graphic);
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
