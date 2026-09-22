# Registro de mudança de procedimento — 2026-09-22

**Origem:** decisão expressa do responsável nesta conversa. **Base técnica conferida:** `hamthet/WinSidebar`, branch `develop`; `build.yml` dispara automaticamente ao fazer push de código e `marketing-assets.yml` dispara na `main` para SVGs selecionados. Repositório FILEBRIDGE identificado por acesso autorizado como um repositório **privado**, com diretório `entregas/` para transferências anteriores; nenhuma release preexistente foi encontrada no momento da consulta. O identificador privado e URLs temporárias de download não são reproduzidos neste arquivo público.

## Decisão aceita

O responsável informou que, até onde sabe, o GitHub Actions está sem créditos. Suspender o uso de Actions para novas builds/testes/distribuições; **não afirmar que o saldo foi conferido**. Usar GitHub CLI + Git no PowerShell local para as operações de repositório e .NET local para build/testes. Caso o responsável precise baixar um executável de teste, entregá-lo pelo FILEBRIDGE privado, conferir o SHA-256 e mantê-lo identificado como prévia, não como release oficial. O guia autocontido, incluindo comandos exemplificativos, restrições de acesso e bloqueios para evitar workflows automáticos, é [`WORKFLOW-LOCAL-FILEBRIDGE.md`](WORKFLOW-LOCAL-FILEBRIDGE.md).

## Ações efetivamente executadas nesta etapa

- Consulta por conector ao repositório privado FILEBRIDGE e ao inventário de workflows do WinSidebar; verificadas existência e permissões de acesso do FILEBRIDGE, não a disponibilidade de créditos de Actions.
- Commit [`7a3d26e`](https://github.com/hamthet/WinSidebar/commit/7a3d26efa1cf40ef2e10196c7dd4cc8d7c4b746f) criou o novo procedimento apenas na `develop`.
- Commit [`8899e4c`](https://github.com/hamthet/WinSidebar/commit/8899e4cbff32fe349ac8c1e3280a978312e41cde) atualizou o índice do dossiê e registrou a precedência da nova política.
- **NÃO EXECUTADO:** comandos `gh`/PowerShell no computador do responsável, compilação local, teste interativo, upload de WinSidebar no FILEBRIDGE, desabilitação de Actions, confirmação de saldo ou publicação de release. Os comandos documentados são instruções, não resultados de testes.

## Estado atualizado e advertências

O quadro de etapas em `LOG.md` foi escrito antes da implementação espanhola e é uma **fotografia histórica**, não o estado atual. A fonte de estado mais recente é [`README.md`](README.md): inglês/português e espanhol têm implementação e testes automatizados antigos aprovados, mas não validação de interface; russo e chinês simplificado estão pendentes. Os PRs #1–#3 continuam rascunhos. A prévia espanhola já gerada via Actions **não foi movida** para FILEBRIDGE. O procedimento novo não altera `main`, a release 1.0.0 ou o conteúdo do produto.

**Próximo bloqueio operacional:** antes de um novo push de código, verificar e, mediante a decisão do responsável, desabilitar os workflows automáticos pertinentes com `gh workflow list --all` e `gh workflow disable`. Registrar a confirmação efetiva. Só então prosseguir com compilação e testes locais, e criar uma prévia FILEBRIDGE quando houver binário verificável.
