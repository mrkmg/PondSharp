import * as PIXI from "pixi.js";

const X_SIZE = 2;
const Y_SIZE = 2;

export class PondRenderer {
    private element: HTMLDivElement;
    private application: PIXI.Application;
    private entities: Record<string, PIXI.Container> = {};
    private fps: PIXI.Text;
    
    public constructor(element: HTMLElement, width: number, height: number) {
        console.log(element);
        this.element = element as HTMLDivElement;
        // noinspection JSSuspiciousNameCombination
        this.application = new PIXI.Application({
            width,
            height
        });
        this.element.appendChild(this.application.view);
        this.application.stage.x = width / 2;
        this.application.stage.y = height / 2;
        this.application.ticker.add(() => this.onTick());
        
        this.fps = new PIXI.Text("FPS: ??", {fill: 0xAAFFAA, fontSize: 12});
        this.fps.x = -width/2;
        this.fps.y = -height/2 + 12;
        this.application.stage.addChild(this.fps);
    }
    
    public getSize() {
        return {
            x: Math.floor(this.application.screen.width / X_SIZE),
            y: Math.floor(this.application.screen.height / Y_SIZE)
        }    
    }
    
    public start = () => this.application.start();
    public stop = () => this.application.stop();
    
    public createEntity(id: string, x: number, y: number, color: number) {
        const graphic = new PIXI.Graphics();
        graphic.beginFill(color);
        graphic.drawRect(0, 0, X_SIZE, Y_SIZE);
        graphic.position.x = x * X_SIZE;
        graphic.position.y = y * Y_SIZE;
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
        container.position.x = x * X_SIZE;
        container.position.y = y * Y_SIZE;
    }
    
    public processEntityChangeRequests(requests: EntityChangeRequest[]) {
        for (const request of requests) {
            if (request.x !== null && request.y !== null) {
                this.moveEntity(request.entityId, request.x, request.y);
            }
        }
    }
    
    private onTick() {
        this.fps.text = `FPS: ${PIXI.Ticker.shared.FPS}`;
    }
}

interface EntityChangeRequest {
    entityId: string;
    x?: number;
    y?: number;
}

