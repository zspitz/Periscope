# Periscope

> A **periscope** is an instrument for observation over, around or through an object, obstacle or condition that prevents direct line-of-sight observation from an observer's current position. ([Wikipedia](https://en.wikipedia.org/wiki/Periscope))

Visual Studio provides an API that enables writing custom visualizers for specific types 

This project provides a common framework for custom debugging visualizers for Visual Studio. It currently provides the following:

* A **VisualizerBaseWindow** that manages the "pass object to debuggee using **TransferObject** / handle returned object in debugger" cycle
* **CopyWatchExpression** command
* Enables reuse of the same UI components outside of a visualizer 

with the following planned features:

* Persistence of window state between debug sessions
* Update notification
