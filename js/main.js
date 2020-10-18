(()=>{"use strict";var t={5305:(t,i,e)=>{var n=e(8244);class o{constructor(t,i,e,o){this.entities={},this.start=()=>this.application.start(),this.stop=()=>this.application.stop(),this.gridSize=o,this.element=t,this.application=new n.Mx({width:i*o,height:i*o}),this.element.appendChild(this.application.view),this.application.stage.x=this.application.view.width/2,this.application.stage.y=this.application.view.height/2,this.application.ticker.add((()=>this.onTick())),this.fps=new n.xv("FPS: ??",{fill:11206570,fontSize:8}),this.fps.x=-this.application.view.width/2,this.fps.y=-this.application.view.height/2,this.application.stage.addChild(this.fps)}createEntity(t,i,e,o){const s=new n.TC;s.beginFill(o),s.drawRect(0,0,this.gridSize,this.gridSize),s.position.x=i*this.gridSize,s.position.y=e*this.gridSize,s.endFill(),this.entities[t]=s,this.application.stage.addChild(s)}destroyEntity(t){this.entities[t].destroy(),this.entities[t]=null,delete this.entities[t]}moveEntity(t,i,e){const n=this.entities[t];n.position.x=i*this.gridSize,n.position.y=e*this.gridSize}changeEntityColor(t,i){const e=this.entities[t];e.clear(),e.beginFill(i),e.drawRect(0,0,this.gridSize,this.gridSize),e.endFill()}processEntityChangeRequests(t){for(const i of t)null!==i.x&&null!==i.y&&this.moveEntity(i.entityId,i.x,i.y),null!=i.color&&this.changeEntityColor(i.entityId,i.color)}onTick(){this.fps.text="FPS: "+Math.round(n.vB.shared.FPS)}}var s=e(7955);class r{constructor(t){this.elementId=t,this.editor=s.j6.create(document.getElementById(t),{value:"",language:"csharp",theme:"vs-dark"})}setCode(t){this.editor.setValue(t)}getCode(){return this.editor.getValue()}}var a=function(t,i,e,n){return new(e||(e=Promise))((function(o,s){function r(t){try{h(n.next(t))}catch(t){s(t)}}function a(t){try{h(n.throw(t))}catch(t){s(t)}}function h(t){var i;t.done?o(t.value):(i=t.value,i instanceof e?i:new e((function(t){t(i)}))).then(r,a)}h((n=n.apply(t,i||[])).next())}))};const h=window;h.ponds={},h.lastPond=0,h.createPond=function(t,i,e,n){return a(this,void 0,void 0,(function*(){h.lastPond++;const s="pond"+h.lastPond;return h.ponds[s]=new o(t,i,e,n),s}))},h.createEditor=function(t,i){return a(this,void 0,void 0,(function*(){h[i]=new r(t)}))}}},i={};function e(n){if(i[n])return i[n].exports;var o=i[n]={id:n,loaded:!1,exports:{}};return t[n].call(o.exports,o,o.exports,e),o.loaded=!0,o.exports}e.m=t,e.n=t=>{var i=t&&t.__esModule?()=>t.default:()=>t;return e.d(i,{a:i}),i},e.d=(t,i)=>{for(var n in i)e.o(i,n)&&!e.o(t,n)&&Object.defineProperty(t,n,{enumerable:!0,get:i[n]})},e.e=()=>Promise.resolve(),e.g=function(){if("object"==typeof globalThis)return globalThis;try{return this||new Function("return this")()}catch(t){if("object"==typeof window)return window}}(),e.o=(t,i)=>Object.prototype.hasOwnProperty.call(t,i),e.r=t=>{"undefined"!=typeof Symbol&&Symbol.toStringTag&&Object.defineProperty(t,Symbol.toStringTag,{value:"Module"}),Object.defineProperty(t,"__esModule",{value:!0})},e.nmd=t=>(t.paths=[],t.children||(t.children=[]),t),e.p="/js/",(()=>{var t={179:0},i=[[5305,216]],n=()=>{};function o(){for(var n,o=0;o<i.length;o++){for(var s=i[o],r=!0,a=1;a<s.length;a++){var h=s[a];0!==t[h]&&(r=!1)}r&&(i.splice(o--,1),n=e(e.s=s[0]))}return 0===i.length&&(e.x(),e.x=()=>{}),n}e.x=()=>{e.x=()=>{},r=r.slice();for(var t=0;t<r.length;t++)s(r[t]);return(n=o)()};var s=o=>{for(var s,r,[h,l,c,d]=o,p=0,u=[];p<h.length;p++)r=h[p],e.o(t,r)&&t[r]&&u.push(t[r][0]),t[r]=0;for(s in l)e.o(l,s)&&(e.m[s]=l[s]);for(c&&c(e),a(o);u.length;)u.shift()();return d&&i.push.apply(i,d),n()},r=self.webpackChunkpondsharp_client=self.webpackChunkpondsharp_client||[],a=r.push.bind(r);r.push=s})(),e.x()})();