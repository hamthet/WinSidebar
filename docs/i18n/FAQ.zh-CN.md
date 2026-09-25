# 常见问题 — WinSidebar 2.0

[English](../FAQ.md) · [Português](FAQ.pt-BR.md) · [Español](FAQ.es-ES.md) · [Русский](FAQ.ru-RU.md)

## 需要安装 WinSidebar 吗？

不需要。下载 ZIP，先解压，然后双击 WinSidebar.exe。

## 需要另外安装 .NET 吗？

不需要。发布版本为自包含程序，所需的 .NET 8 runtime 已包含在 WinSidebar.exe 中。

## Windows 显示了警告，正常吗？

当前可执行文件未进行数字签名，因此 SmartScreen 或组织安全策略可能显示警告。

## WinSidebar.exe 应该放在哪里？

放在你有权限管理的任意文件夹。请先解压 ZIP，不要直接从压缩包内部运行。

## 如何打开或隐藏侧边栏？

按 AltGr+Y，或点击屏幕边缘的窄标签。

## 如何修改快捷方式？

右键编辑；左键打开。

## Scripts 是什么？

它们是可重复使用的纯文本片段，不会执行代码。点击后会尝试把保存的文本粘贴到你刚才使用的外部应用。

## 设置保存在哪里？

%LOCALAPPDATA%\WinSidebar。可执行文件与保存的配置相互独立。

## 删除 WinSidebar.exe 会丢失快捷方式吗？

不会。只删除可执行文件不会删除配置文件。

## 如何彻底删除全部内容？

退出 WinSidebar，删除 WinSidebar.exe；只有在同时希望删除快捷方式、文本片段、忽略的应用、图标和偏好设置时，才删除 %LOCALAPPDATA%\WinSidebar。

## 为什么在以管理员身份运行的程序中可能无法粘贴？

Windows 可能阻止普通权限程序向更高权限进程模拟输入。

## WinSidebar 会发送遥测或自动启动吗？

不会。2.0 不会执行这两项操作。