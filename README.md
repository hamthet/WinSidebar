# WinSidebar

Barra lateral retrátil para alternar janelas e abrir quatro atalhos configuráveis, com visual inspirado no Windows 98.

**[Baixar para Windows 10/11 x64 (Releases)](https://github.com/hamthet/WinSidebar/releases)**

## Instalação

Baixe `WinSidebar-v1.0.0-win-x64.zip` na página Releases, extraia e execute `WinSidebar.exe`. Não é necessário PowerShell, Git, compilador, instalador, conta ou download separado do .NET. O executável da edição distribuída inclui o runtime .NET 8. Não exige administrador em uma pasta do usuário; políticas de segurança podem impedir a execução de software não assinado.

## Uso

A aba lateral expande/recolhe. `Shift+F1` alterna a barra; `Shift+F2/F3` percorrem janelas e `Shift+F4` ativa a selecionada. Na faixa **PASTAS / WEB**, clique na engrenagem e depois em um dos quatro atalhos para alterar nome, pasta/URL, ícone e navegador. Os padrões são **Documentos**, **Downloads**, **Acervo não configurado** e **Google**. No topo, use mover, diminuir, aumentar ou X para encerrar.

As preferências são salvas apenas em `%LOCALAPPDATA%\WinSidebar`, isoladas de outras instalações. O programa não altera o navegador padrão, não cria inicialização automática e não envia telemetria. Caso tenha outra barra de janelas aberta, feche-a antes de iniciar o WinSidebar para evitar disputa pelas teclas globais. O WinSidebar não desinstala nem substitui outro aplicativo.

Para remover: encerre o programa e apague o executável; opcionalmente apague `%LOCALAPPDATA%\WinSidebar` para excluir as preferências.

**Desenvolvimento:** o projeto C# está em `src/`, com compilação reprodutível pelo fluxo de CI. Não publique arquivos pessoais, configurações locais ou material de repositórios históricos nesta distribuição.
