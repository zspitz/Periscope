# Periscope

> A **periscope** is an instrument for observation over, around or through an object, obstacle or condition that prevents direct line-of-sight observation from an observer's current position. ([Wikipedia](https://en.wikipedia.org/wiki/Periscope))

Visual Studio provides an API that enables writing custom visualizers for specific types. The custom visualizer can focus on a specific object which exists in the debuggee side, and bring certain parts into better focus on the debugger side.

This project provides a common framework for custom debugging visualizers for Visual Studio. It currently provides the following:

* A **VisualizerBaseWindow** that manages the "pass object to debuggee using **TransferObject** / handle returned object in debugger" cycle
* **CopyWatchExpression** command
* Enables reuse of the same UI components outside of a visualizer 

with the following planned features:

* Persistence of window state between debug sessions
* Update notification
* Access to more detail about the current source-code debugging environment, such as the [source expression](https://stackoverflow.com/questions/54749716/visualized-expression-in-custom-data-visualizer) or the [language of the source file](https://stackoverflow.com/questions/55954016/detect-source-language-at-runtime-from-within-debugging-visualizer).

This framework will be used in the following visualizers:

| Visualizer | targets | status | Periscope usage |
| --- | --- | --- | --- |
| [Expression Tree Visualizer](https://github.com/zspitz/ExpressionTreeVisualizer) | Expression trees etc. | Available | In progress |
| [ANTLR Parse Tree Visualizer](https://github.com/zspitz/ANTLR4ParseTreeVisualizer) | ANTLR parse trees | Available | Pending |
| Type hierarchy visualizer | `System.Type` | Planned | Pending |
| Roslyn Syntax Node visualizer | Roslyn syntax nodes | Planned | Pending |
