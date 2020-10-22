import * as PIXI from "pixi.js";

export class PondRenderer {
    private element: HTMLDivElement;
    private application: PIXI.Application;
    private entities: Record<string, PIXI.Container> = {};
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
    
    public createEntity(id: string, x: number, y: number, color: number) {
        const graphic = new PIXI.Graphics();
        graphic.beginFill(color);
        graphic.drawRect(0, 0, this.gridSize, this.gridSize);
        graphic.position.x = x * this.gridSize;
        graphic.position.y = y * this.gridSize;
        graphic.endFill()
        this.entities[id] = graphic;
        this.application.stage.addChild(graphic);
    }
    
    public destroyEntity(id: string) {
        this.entities[id].destroy();
        this.entities[id] = null;
        delete this.entities[id];
    }
    
    public moveEntity(id: string, x: number, y: number) {
        const container = this.entities[id];
        container.position.x = x * this.gridSize;
        container.position.y = y * this.gridSize;
    }
    
    public changeEntityColor(id: string, color: number) {
        const graphic = this.entities[id] as PIXI.Graphics;
        graphic.clear();
        graphic.beginFill(color);
        graphic.drawRect(0, 0, this.gridSize, this.gridSize);
        graphic.endFill();
    }
    
    public processEntityChangeRequests(requests: EntityChangeRequest[]) {
        for (const request of requests) {
            if (request.X !== null && request.Y !== null) {
                this.moveEntity(request.EntityId, request.X, request.Y);
            }
            if (request.Color != null) {
                this.changeEntityColor(request.EntityId, request.Color);
            }
        }
    }
    
    private onTick() {
        this.fps.text = `FPS: ${Math.round(PIXI.Ticker.shared.FPS)}`;
    }
}

interface EntityChangeRequest {
    EntityId: string;
    X?: number;
    Y?: number;
    Color?: number;
}

