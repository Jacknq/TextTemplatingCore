module.exports=(()=>{"use strict";var e={112:(e,t,n)=>{Object.defineProperty(t,"__esModule",{value:!0}),t.deactivate=t.activate=void 0;const o=n(549),s=n(622);t.activate=function(e){console.log('Congratulations, your extension "texttemplating" is now active!');let t=o.commands.registerCommand("tt.helloWorlds",(()=>{o.window.showInformationMessage("Hello World from texttemplating!")}));e.subscriptions.push(o.commands.registerCommand("tt.helloWorld",(e=>{const t=o.window.terminals.map((e=>({label:`${e.name}`,terminal:e}))),n=`TT #${s.basename(e.fsPath)}`;var a,r=t.find((e=>e.label==n));(a=null!=r?r.terminal:o.window.createTerminal({name:n,cwd:s.dirname(e.fsPath)})).sendText(`dotnet tt ${s.basename(e.fsPath)}`),null==r&&a.show()}))),e.subscriptions.push(o.commands.registerCommand("terminalTest.show",(()=>{(0!==o.window.terminals.length||(o.window.showErrorMessage("No active terminals"),0))&&function(){const e=o.window.terminals.map((e=>({label:`name: ${e.name}`,terminal:e})));return o.window.showQuickPick(e).then((e=>e?e.terminal:void 0))}().then((e=>{e&&e.show()}))}))),e.subscriptions.push(t)},t.deactivate=function(){}},622:e=>{e.exports=require("path")},549:e=>{e.exports=require("vscode")}},t={};return function n(o){if(t[o])return t[o].exports;var s=t[o]={exports:{}};return e[o](s,s.exports,n),s.exports}(112)})();