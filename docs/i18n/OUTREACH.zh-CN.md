# WinSidebar 2.0.0 — 发布说明

[README](README.zh-CN.md) · [English](../../docs/OUTREACH.md) · [Português](OUTREACH.pt-BR.md) · [Español](OUTREACH.es-ES.md) · [Русский](OUTREACH.ru-RU.md)

## 建议的简短介绍

WinSidebar 2.0.0 是面向 Windows 10/11 x64 的便携侧边栏，可用于切换打开的窗口、启动最多 12 个快捷方式并粘贴最多 8 个可重复使用的文本片段。它以单个自包含可执行文件发布，支持英语、巴西葡萄牙语、西班牙语、俄语和简体中文，并在新配置文件中默认使用英语。

主要按键：AltGr+Y 打开/收起侧边栏，F1–F4 打开快捷方式 1–4，Shift+F1–F4 是文本片段的默认快捷键。

## 视觉资源

社交发布可使用 ../../assets/i18n/zh-CN/linkedin-illustration.svg，项目展示可使用 hero-illustration.svg。

这些是概念插图，不是应用截图。导出或裁剪时应保留这一说明。

## 必须保持准确的表述

- “便携、自包含”不等于“经过数字签名”。
- .NET 8 runtime 已包含。
- 支持五种运行时语言，新配置默认使用英语。
- 文本片段只保存和粘贴纯文本，不执行脚本。
- 窗口发现使用启发式判断，不应宣传为与 Alt+Tab 完全一致。
- WinSidebar 不发送遥测，不更改默认浏览器，也不设置自动启动。

官方源代码：https://github.com/hamthet/WinSidebar