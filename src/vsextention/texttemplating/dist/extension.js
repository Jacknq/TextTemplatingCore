module.exports=(()=>{"use strict";var t={112:(t,e,o)=>{Object.defineProperty(e,"__esModule",{value:!0}),e.deactivate=e.activate=void 0;const n=o(549);e.activate=function(t){let e=1;console.log('Congratulations, your extension "texttemplating" is now active!');let o=n.commands.registerCommand("tt.helloWorlds",(()=>{n.window.showInformationMessage("Hello World from texttemplating!")}));t.subscriptions.push(n.commands.registerCommand("tt.helloWorld",(t=>{const o=n.window.createTerminal("TT #"+e++);o.sendText(`dotnet tt '${t.fsPath}'`),o.show()}))),t.subscriptions.push(n.commands.registerCommand("terminalTest.show",(()=>{(0!==n.window.terminals.length||(n.window.showErrorMessage("No active terminals"),0))&&function(){const t=n.window.terminals.map((t=>({label:`name: ${t.name}`,terminal:t})));return n.window.showQuickPick(t).then((t=>t?t.terminal:void 0))}().then((t=>{t&&t.show()}))}))),t.subscriptions.push(o)},e.deactivate=function(){}},549:t=>{t.exports=require("vscode")}},e={};return function o(n){if(e[n])return e[n].exports;var s=e[n]={exports:{}};return t[n](s,s.exports,o),s.exports}(112)})();