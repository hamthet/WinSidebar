# Tutorial do WinSidebar 2.0.0

[README](README.pt-BR.md) · [English](../../docs/TUTORIAL.md) · [Español](TUTORIAL.es-ES.md) · [Русский](TUTORIAL.ru-RU.md) · [简体中文](TUTORIAL.zh-CN.md)

## 1. Inicie o aplicativo portátil

Extraia o ZIP do WinSidebar 2.0.0 e execute WinSidebar.exe. Não é necessário instalador nem runtime .NET separado.

Em um perfil totalmente novo, English vem pré-selecionado. O seletor inicial também oferece Português (Brasil), Español, Русский e 简体中文. A escolha fica salva em %LOCALAPPDATA%\WinSidebar.

Se o Windows exibir um aviso do SmartScreen, lembre que o executável atual não possui assinatura digital. Siga a política de segurança da sua organização.

## 2. Abra e recolha a barra

Use qualquer método:

- clique na aba estreita presa à borda da tela;
- pressione AltGr+Y.

AltGr+Y é uma alternância: recolhida vira aberta; aberta vira recolhida.

Os controles do cabeçalho permitem mudar a barra de lado e percorrer larguras e alturas disponíveis. Essas escolhas persistem.

## 3. Alterne entre janelas

Abra a barra e clique em uma janela elegível da lista. O WinSidebar agrupa as janelas listadas por monitor.

Clique com o botão direito em uma janela para:

- Renomear — aplicar um rótulo temporário à janela viva;
- Restaurar nome — remover esse rótulo temporário;
- Ignorar este aplicativo — ocultar de forma persistente janelas daquele aplicativo.

Use o menu de contexto geral para gerenciar os aplicativos ignorados e exibi-los novamente.

A descoberta usa metadados e heurísticas do Windows. Alguns programas podem expor janelas superiores incomuns que não coincidem exatamente com a lista de Alt+Tab do sistema.

## 4. Configure atalhos

A seção Atalhos começa com quatro itens.

- Clique normal: abre a pasta ou site.
- Botão direito: edita o atalho.
- +: adiciona uma linha de quatro.
- −: remove a última linha adicionada, nunca abaixo de quatro.
- Restaurar: restaura somente a seção de atalhos após confirmação.

O limite do produto é 12 atalhos.

O editor permite configurar nome, destino de pasta/site, ícone e comportamento do navegador. Ícones personalizados são copiados para o perfil do WinSidebar.

F1–F4 abrem globalmente os atalhos 1–4. Os demais atalhos são acionados pelo mouse.

## 5. Configure snippets de texto

A seção Scripts contém snippets de texto literal. Eles não são scripts executáveis.

- Clique na linha: cola o texto salvo no aplicativo externo elegível que estava ativo por último.
- Engrenagem: edita nome, conteúdo e hotkey.
- +: adiciona um snippet.
- −: remove o último snippet adicionado, nunca abaixo de quatro.
- Restaurar: restaura somente os snippets após confirmação.

O limite é 8 snippets.

Os snippets 1–4 usam Shift+F1–F4 por padrão. Cada snippet pode usar Nenhum ou Shift+F1 até Shift+F12. Duplicidades são rejeitadas.

O WinSidebar usa temporariamente a área de transferência do Windows para a colagem e depois tenta restaurar o conteúdo anterior. A entrada pode falhar se o aplicativo-alvo estiver em nível de integridade superior, como um processo elevado de administrador.

## 6. Troque o idioma

Abra o menu de contexto geral e escolha Idioma. Os cinco idiomas suportados são:

- English
- Português (Brasil)
- Español
- Русский
- 简体中文

A escolha entra em vigor no aplicativo e é salva em settings.ini.

Instalações novas usam inglês como padrão, independentemente do idioma de exibição do Windows. Perfis antigos criados antes da configuração de idioma podem inicialmente permanecer em português.

## 7. Arquivos do perfil

O estado do usuário fica em:

    %LOCALAPPDATA%\WinSidebar

Conforme os recursos usados, a pasta pode conter settings.ini, shortcuts.xml, snippets.json, ignored-apps.json, icons\ e arquivos .bak.

Dados do usuário, como nomes de atalhos, caminhos, conteúdo de snippets e títulos de janelas externas, nunca são traduzidos.

## 8. Restaurar e recuperar

Restaurar Atalhos e Restaurar Scripts são independentes. Restaurar uma seção não redefine a outra.

Se um arquivo do perfil ficar ilegível, preserve o original antes de excluir ou substituir manualmente. Um arquivo malformado pode conter evidência útil para recuperação.

Migração de preferências entre versões não faz parte do contrato de validação da 2.0.0.

## 9. Remover

Encerre o WinSidebar e apague WinSidebar.exe e a documentação extraída.

O perfil em %LOCALAPPDATA%\WinSidebar é independente. Apague-o somente se também quiser remover atalhos, snippets, aplicativos ignorados, ícones e preferências.

## 10. Referência de teclado

| Comando | Ação |
| --- | --- |
| AltGr+Y | Abrir/recolher a barra |
| F1–F4 | Abrir atalhos 1–4 |
| Shift+F1–F4 | Hotkeys padrão dos snippets 1–4 |
| Shift+F1–F12 | Hotkeys configuráveis disponíveis |

A antiga navegação de janelas por Shift+F foi removida intencionalmente na 2.0.0.