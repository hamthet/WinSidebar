# Notas de lançamento do WinSidebar 2.0.0

[English](../RELEASE-NOTES.md) · [Español](RELEASE-NOTES.es-ES.md) · [Русский](RELEASE-NOTES.ru-RU.md) · [简体中文](RELEASE-NOTES.zh-CN.md)

A versão 2.0.0 é uma revisão ampla do produto.

## Destaques

- Runtime completo em inglês, português brasileiro, espanhol, russo e chinês simplificado.
- Inglês é o padrão de instalações novas; a escolha de idioma é persistida.
- Executável único autocontido .NET 8 para Windows x64.
- De 4 a 12 atalhos configuráveis de pasta/site, com +, − e restauração independentes.
- De 4 a 8 snippets de texto literal, com controles independentes.
- Hotkeys de snippets: Nenhum ou Shift+F1 até Shift+F12; snippets 1–4 usam Shift+F1–F4 por padrão.
- F1–F4 abrem os atalhos 1–4.
- AltGr+Y abre/recolhe a barra.
- Edição de atalho por botão direito.
- Menu de janela para renomear/restaurar temporariamente e ignorar aplicativos de forma persistente.
- Ciclos persistidos de largura/altura e lado da barra.
- README, tutorial, início rápido, divulgação e arte conceitual em cinco idiomas.

## Compatibilidade

A antiga navegação de janelas por Shift+F foi removida. Perfis legados sem idioma salvo podem permanecer em português até uma escolha explícita. Migração de preferências entre versões está fora do escopo de validação da 2.0.0.

## Segurança e distribuição

O executável é portátil e não assinado. SmartScreen ou políticas corporativas podem alertar. WinSidebar não ativa inicialização automática, não muda o navegador padrão e não envia telemetria.

Snippets usam temporariamente a área de transferência para colagem e tentam restaurar o conteúdo anterior.