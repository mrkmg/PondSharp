import * as PIXI from "pixi.js";

export class PondRenderer {
    private element: HTMLDivElement;
    private application: PIXI.Application;
    private entities: Record<string, PIXI.Container> = {};
    private readonly fps: PIXI.Text;
    private readonly gridSize: number;
    
    public constructor(element: HTMLElement, width: number, height: number, gridSize: number) {
        this.gridSize = gridSize;
        this.element = element as HTMLDivElement;
        // noinspection JSSuspiciousNameCombination
        this.application = new PIXI.Application({
            width: width * gridSize,
            height: width * gridSize
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
    
    public start = () => this.application.start();
    public stop = () => this.application.stop();
    
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
            if (request.x !== null && request.y !== null) {
                this.moveEntity(request.entityId, request.x, request.y);
            }
            if (request.color != null) {
                this.changeEntityColor(request.entityId, request.color);
            }
        }
    }
    
    private onTick() {
        this.fps.text = `FPS: ${Math.round(PIXI.Ticker.shared.FPS)}`;
    }
}

interface EntityChangeRequest {
    entityId: string;
    x?: number;
    y?: number;
    color?: number;
}

