# ILXTime
ILXTime是一款基于Mono.Cecil和ILRuntime的纯C#热修复方案
通过修改IL代码, 在每个函数中注入检测, 然后在运行时通过ILRuntime绑定指定的函数, 从而实现热修复函数

ILXTime的优势在于无侵入性, 可以在不修改原有代码的情况下进行HotFix

ILRuntime: https://github.com/Ourpalm/ILRuntime
