/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ({

/***/ 5305:
/***/ ((__unused_webpack_module, __unused_webpack___webpack_exports__, __webpack_require__) => {


// EXTERNAL MODULE: ./node_modules/pixi.js/lib/pixi.es.js + 37 modules
var pixi_es = __webpack_require__(8244);
// CONCATENATED MODULE: ./ts/pond/pond-renderer.ts

const win = window;
class PondRenderer {
    constructor(pondRef, element, width, height, gridSize) {
        this.entities = {};
        this.isMouseDown = false;
        this.start = () => this.application.start();
        this.stop = () => this.application.stop();
        this.pondRef = pondRef;
        this.element = element;
        this.width = width;
        this.height = height;
        this.gridSize = gridSize;
        this.init();
    }
    init() {
        if (this.application) {
            console.debug("Removing PIXI instance");
            this.application.destroy(true, { children: true, texture: true, baseTexture: true });
            this.element.removeChild(this.element.children.item(0));
            this.application = null;
        }
        console.debug("Creating PIXI instance");
        // noinspection JSSuspiciousNameCombination
        this.application = new pixi_es/* Application */.Mx({
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
        this.entityHolder = new pixi_es/* Container */.W2();
        this.application.stage.addChild(this.entityHolder);
        this.fps = new pixi_es/* Text */.xv("FPS: ??", { fill: 0xAAFFAA, fontSize: 8 });
        this.fps.x = -this.application.view.width / 2;
        this.fps.y = -this.application.view.height / 2;
        this.application.stage.addChild(this.fps);
        this.cursor = new pixi_es/* Graphics */.TC();
        this.cursor.beginFill(0xFFFFFF);
        this.cursor.drawCircle(this.gridSize / 2, this.gridSize / 2, this.gridSize / 2);
        this.cursor.endFill();
        this.cursor.position.x = 0;
        this.cursor.position.y = 0;
        this.application.stage.addChild(this.cursor);
        this.application.stage.interactive = true;
        this.application.view.addEventListener("mousemove", e => this.onMouseMove(e));
        this.application.view.addEventListener("mousedown", e => this.onMouseDown(e));
        this.application.view.addEventListener("mouseup", e => this.onMouseUp(e));
    }
    onMouseMove(e) {
        const gridLocation = this.getGridPosition(e);
        this.cursor.position.x = gridLocation.x * this.gridSize;
        this.cursor.position.y = gridLocation.y * this.gridSize;
        if (this.isMouseDown && (this.lastMouseLocation.x !== gridLocation.x || this.lastMouseLocation.y !== gridLocation.y)) {
            this.onClick(gridLocation.x, gridLocation.y);
        }
        this.lastMouseLocation = gridLocation;
    }
    onMouseDown(e) {
        e.stopPropagation();
        this.isMouseDown = true;
        const gridLocation = this.getGridPosition(e);
        this.lastMouseLocation = gridLocation;
        this.onClick(gridLocation.x, gridLocation.y);
    }
    onMouseUp(e) {
        e.stopPropagation();
        this.isMouseDown = false;
    }
    onClick(x, y) {
        this.pondRef.invokeMethodAsync("OnClick", x, y);
    }
    getGridPosition(e) {
        const stageXRaw = e.offsetX - this.application.stage.x;
        const stageYRaw = e.offsetY - this.application.stage.y;
        return {
            x: Math.round(stageXRaw / this.gridSize),
            y: Math.round(stageYRaw / this.gridSize)
        };
    }
    onTick() {
        this.fps.text = `${Math.round(pixi_es/* Ticker.shared.FPS */.vB.shared.FPS)} fps`;
    }
    resize(width, height, gridSize) {
        this.width = width;
        this.height = height;
        this.gridSize = gridSize;
        this.gridSize = gridSize;
        this.init();
    }
    createEntity(id, x, y, color) {
        const graphic = new pixi_es/* Graphics */.TC();
        graphic.beginFill(color);
        graphic.drawCircle(this.gridSize / 2, this.gridSize / 2, this.gridSize / 2);
        graphic.endFill();
        graphic.position.x = x * this.gridSize;
        graphic.position.y = y * this.gridSize;
        this.entities[id] = graphic;
        this.entityHolder.addChild(graphic);
    }
    destroyEntity(id) {
        if (!this.entities[id])
            return;
        this.entities[id].destroy();
        this.entities[id] = null;
        delete this.entities[id];
    }
    moveEntity(id, x, y) {
        if (!this.entities[id])
            return;
        const container = this.entities[id];
        container.position.x = x * this.gridSize;
        container.position.y = y * this.gridSize;
    }
    changeEntityColor(id, color) {
        if (!this.entities[id])
            return;
        const graphic = this.entities[id];
        graphic.clear();
        graphic.beginFill(color);
        graphic.drawCircle(this.gridSize / 2, this.gridSize / 2, this.gridSize / 2);
        graphic.endFill();
    }
    processEntityChangeRequestsRaw(pointer) {
        const length = win.Blazor.platform.getArrayLength(pointer);
        for (let i = 0; i < length; i++) {
            const entryPtr = win.Blazor.platform.getArrayEntryPtr(pointer, i, 16);
            const id = win.Blazor.platform.readInt32Field(entryPtr, 0);
            if (!this.entities[id])
                continue;
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

// EXTERNAL MODULE: ./node_modules/monaco-editor/esm/vs/editor/editor.main.js + 264 modules
var editor_main = __webpack_require__(7955);
// CONCATENATED MODULE: ./ts/code/code-editor.ts

class CodeEditor {
    constructor(elementId) {
        this.elementId = elementId;
        this.editor = editor_main/* editor.create */.j6.create(document.getElementById(elementId), {
            value: "",
            language: "csharp",
            theme: "vs-dark",
            automaticLayout: true
        });
    }
    setCode(code) {
        this.editor.setValue(code);
    }
    getCode() {
        return this.editor.getValue();
    }
}

// CONCATENATED MODULE: ./ts/main.ts
var __awaiter = (undefined && undefined.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};


const main_win = window;
main_win.ponds = {};
main_win.lastPond = 0;
main_win.createPond = function (pondRef, element, width, height, gridSize) {
    return __awaiter(this, void 0, void 0, function* () {
        main_win.lastPond++;
        const id = `pond_${main_win.lastPond}`;
        main_win.ponds[id] = new PondRenderer(pondRef, element, width, height, gridSize);
        return id;
    });
};
main_win.createEditor = function (elementId, globalName) {
    return __awaiter(this, void 0, void 0, function* () {
        main_win[globalName] = new CodeEditor(elementId);
    });
};
main_win.getSize = () => [main_win.innerWidth, main_win.innerHeight];


/***/ })

/******/ 	});
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		if(__webpack_module_cache__[moduleId]) {
/******/ 			return __webpack_module_cache__[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			id: moduleId,
/******/ 			loaded: false,
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		__webpack_modules__[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Flag the module as loaded
/******/ 		module.loaded = true;
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = __webpack_modules__;
/******/ 	
/************************************************************************/
/******/ 	/* webpack/runtime/compat get default export */
/******/ 	(() => {
/******/ 		// getDefaultExport function for compatibility with non-harmony modules
/******/ 		__webpack_require__.n = (module) => {
/******/ 			var getter = module && module.__esModule ?
/******/ 				() => module['default'] :
/******/ 				() => module;
/******/ 			__webpack_require__.d(getter, { a: getter });
/******/ 			return getter;
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/define property getters */
/******/ 	(() => {
/******/ 		// define getter functions for harmony exports
/******/ 		__webpack_require__.d = (exports, definition) => {
/******/ 			for(var key in definition) {
/******/ 				if(__webpack_require__.o(definition, key) && !__webpack_require__.o(exports, key)) {
/******/ 					Object.defineProperty(exports, key, { enumerable: true, get: definition[key] });
/******/ 				}
/******/ 			}
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/ensure chunk */
/******/ 	(() => {
/******/ 		// The chunk loading function for additional chunks
/******/ 		// Since all referenced chunks are already included
/******/ 		// in this file, this function is empty here.
/******/ 		__webpack_require__.e = () => Promise.resolve();
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/global */
/******/ 	(() => {
/******/ 		__webpack_require__.g = (function() {
/******/ 			if (typeof globalThis === 'object') return globalThis;
/******/ 			try {
/******/ 				return this || new Function('return this')();
/******/ 			} catch (e) {
/******/ 				if (typeof window === 'object') return window;
/******/ 			}
/******/ 		})();
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/hasOwnProperty shorthand */
/******/ 	(() => {
/******/ 		__webpack_require__.o = (obj, prop) => Object.prototype.hasOwnProperty.call(obj, prop)
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/make namespace object */
/******/ 	(() => {
/******/ 		// define __esModule on exports
/******/ 		__webpack_require__.r = (exports) => {
/******/ 			if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 				Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 			}
/******/ 			Object.defineProperty(exports, '__esModule', { value: true });
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/node module decorator */
/******/ 	(() => {
/******/ 		__webpack_require__.nmd = (module) => {
/******/ 			module.paths = [];
/******/ 			if (!module.children) module.children = [];
/******/ 			return module;
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/publicPath */
/******/ 	(() => {
/******/ 		__webpack_require__.p = "js/";
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/jsonp chunk loading */
/******/ 	(() => {
/******/ 		// no baseURI
/******/ 		
/******/ 		// object to store loaded and loading chunks
/******/ 		// undefined = chunk not loaded, null = chunk preloaded/prefetched
/******/ 		// Promise = chunk loading, 0 = chunk loaded
/******/ 		var installedChunks = {
/******/ 			179: 0
/******/ 		};
/******/ 		
/******/ 		var deferredModules = [
/******/ 			[5305,216]
/******/ 		];
/******/ 		// no chunk on demand loading
/******/ 		
/******/ 		// no prefetching
/******/ 		
/******/ 		// no preloaded
/******/ 		
/******/ 		// no HMR
/******/ 		
/******/ 		// no HMR manifest
/******/ 		
/******/ 		var checkDeferredModules = () => {
/******/ 		
/******/ 		};
/******/ 		function checkDeferredModulesImpl() {
/******/ 			var result;
/******/ 			for(var i = 0; i < deferredModules.length; i++) {
/******/ 				var deferredModule = deferredModules[i];
/******/ 				var fulfilled = true;
/******/ 				for(var j = 1; j < deferredModule.length; j++) {
/******/ 					var depId = deferredModule[j];
/******/ 					if(installedChunks[depId] !== 0) fulfilled = false;
/******/ 				}
/******/ 				if(fulfilled) {
/******/ 					deferredModules.splice(i--, 1);
/******/ 					result = __webpack_require__(__webpack_require__.s = deferredModule[0]);
/******/ 				}
/******/ 			}
/******/ 			if(deferredModules.length === 0) {
/******/ 				__webpack_require__.x();
/******/ 				__webpack_require__.x = () => {
/******/ 		
/******/ 				}
/******/ 			}
/******/ 			return result;
/******/ 		}
/******/ 		__webpack_require__.x = () => {
/******/ 			// reset startup function so it can be called again when more startup code is added
/******/ 			__webpack_require__.x = () => {
/******/ 		
/******/ 			}
/******/ 			chunkLoadingGlobal = chunkLoadingGlobal.slice();
/******/ 			for(var i = 0; i < chunkLoadingGlobal.length; i++) webpackJsonpCallback(chunkLoadingGlobal[i]);
/******/ 			return (checkDeferredModules = checkDeferredModulesImpl)();
/******/ 		};
/******/ 		
/******/ 		// install a JSONP callback for chunk loading
/******/ 		var webpackJsonpCallback = (data) => {
/******/ 			var [chunkIds, moreModules, runtime, executeModules] = data;
/******/ 			// add "moreModules" to the modules object,
/******/ 			// then flag all "chunkIds" as loaded and fire callback
/******/ 			var moduleId, chunkId, i = 0, resolves = [];
/******/ 			for(;i < chunkIds.length; i++) {
/******/ 				chunkId = chunkIds[i];
/******/ 				if(__webpack_require__.o(installedChunks, chunkId) && installedChunks[chunkId]) {
/******/ 					resolves.push(installedChunks[chunkId][0]);
/******/ 				}
/******/ 				installedChunks[chunkId] = 0;
/******/ 			}
/******/ 			for(moduleId in moreModules) {
/******/ 				if(__webpack_require__.o(moreModules, moduleId)) {
/******/ 					__webpack_require__.m[moduleId] = moreModules[moduleId];
/******/ 				}
/******/ 			}
/******/ 			if(runtime) runtime(__webpack_require__);
/******/ 			parentChunkLoadingFunction(data);
/******/ 			while(resolves.length) {
/******/ 				resolves.shift()();
/******/ 			}
/******/ 		
/******/ 			// add entry modules from loaded chunk to deferred list
/******/ 			if(executeModules) deferredModules.push.apply(deferredModules, executeModules);
/******/ 		
/******/ 			// run deferred modules when all chunks ready
/******/ 			return checkDeferredModules();
/******/ 		}
/******/ 		
/******/ 		var chunkLoadingGlobal = self["webpackChunkpondsharp_client"] = self["webpackChunkpondsharp_client"] || [];
/******/ 		var parentChunkLoadingFunction = chunkLoadingGlobal.push.bind(chunkLoadingGlobal);
/******/ 		chunkLoadingGlobal.push = webpackJsonpCallback;
/******/ 	})();
/******/ 	
/************************************************************************/
/******/ 	// run startup
/******/ 	return __webpack_require__.x();
/******/ })()
;
//# sourceMappingURL=main.js.map