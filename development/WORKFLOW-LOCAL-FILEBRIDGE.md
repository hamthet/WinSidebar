# WinSidebar — desenvolvimento local e distribuição de prévias via FILEBRIDGE

**Decisão do responsável — 2026-09-22.** Este procedimento substitui qualquer orientação anterior que dependa de GitHub Actions para gerar, testar ou disponibilizar novos builds. O responsável informou que acredita não haver créditos de Actions; **a situação da cobrança não foi verificada**. Não executar ou disparar workflows enquanto essa restrição vigorar. O histórico de Actions já concluídos continua válido como evidência histórica, não como infraestrutura disponível para o futuro.

**Limite operacional:** o assistente neste chat não executa comandos no PowerShell do computador do responsável. Os comandos abaixo são para execução local no Windows, com GitHub CLI (`gh`), Git e .NET SDK instalados. Um commit feito pelo conector do GitHub neste chat NÃO significa que um comando `gh`, um build local ou um teste de interface tenha sido executado. Registrar autor, ambiente, SHA, saída e resultados reais de cada etapa; nunca marcar um procedimento descrito como `PASS`.

## Princípios e canais

1. **Código e planejamento:** `hamthet/WinSidebar`. O dossiê `development/**` pertence somente à branch pública `develop`; implementação em branches de recurso e PRs em rascunho, sem merge prematuro em `main`.
2. **Operações de repositório:** `gh` e `git` no PowerShell local para autenticação, fetch, branch, push, PR, inspeção de commits e download; `dotnet` local para compilação/testes. O GitHub CLI não substitui o compilador nem valida janelas reais por si só.
3. **Binários de teste:** o repositório **privado** FILEBRIDGE, separado do WinSidebar público. Utilizar uma prerelease de desenvolvimento no FILEBRIDGE com EXE e soma SHA-256, sem criar release oficial do WinSidebar nem incluir o dossiê no arquivo. Uma prerelease privada exige autenticação e permissão de acesso para baixar; nunca apresentar sua URL como download público. O nome completo do repositório privado será fornecido localmente via variável, não armazenado neste dossiê público.
4. **Validação:** testes automatizados locais, verificação de hash após download e teste manual do responsável no Windows. Windows Alt+Tab, janelas fantasmas, cinco Calculadoras, ignorados, renomeação, Save/Restore, idiomas e DPI exigem testes reais; não substituir pela compilação. Testes de migração entre versões são `OUT OF SCOPE` por decisão do responsável.

## Antes de fazer novos pushes de código

O workflow `build.yml` do WinSidebar possui gatilho automático `push` para alterações em `src/**`, no `.csproj` e outros arquivos. Existem também outros workflows. **Não presumir que parar de chamar `gh workflow run` impeça execuções automáticas.** Consultar os workflows e desabilitar os pertinentes antes de alterar código enquanto os créditos não estiverem disponíveis:

```powershell
$ErrorActionPreference = 'Stop'
gh auth status
gh workflow list --all --repo hamthet/WinSidebar
# Conferir os nomes/IDs na lista e desabilitar individualmente os workflows pertinentes:
# gh workflow disable <WORKFLOW_ID_OU_ARQUIVO> --repo hamthet/WinSidebar
# Confirmar novamente com: gh workflow list --all --repo hamthet/WinSidebar
```

`gh workflow disable` modifica a disponibilidade de Actions no repositório, portanto só executar se essa for a decisão administrativa do responsável. Caso não haja permissão para desabilitar ou a desativação não seja confirmada, evitar pushes que acionem workflows. Registrar em `development/LOG.md` o resultado efetivo e reavaliar antes de retomar Actions. **Neste registro não se afirma que workflows foram desativados.**

## Preparar e validar uma branch no Windows (PowerShell)

Exemplo baseado na última etapa já existente; mudar explicitamente a branch para a etapa em curso, sem empurrar alterações para `main`:

```powershell
$ErrorActionPreference = 'Stop'
gh auth status
gh repo clone hamthet/WinSidebar
Set-Location .\WinSidebar
git fetch origin --prune
git switch --track origin/feature/runtime-i18n-es
# Se a branch já existir localmente: git switch feature/runtime-i18n-es; git pull --ff-only
$commit = (git rev-parse HEAD).Trim()
git status --short

dotnet --version
dotnet run --project tests/LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Falha nos testes locais de idioma.' }

dotnet publish WinSidebar.csproj -c Release -r win-x64 --self-contained true -o .\publish-preview
if ($LASTEXITCODE -ne 0) { throw 'Falha na publicação local.' }
$exe = (Resolve-Path .\publish-preview\WinSidebar.exe).Path
$sha256 = (Get-FileHash $exe -Algorithm SHA256).Hash.ToLowerInvariant()
"Commit: $commit; EXE SHA-256: $sha256"
```

A amostra utiliza a branch espanhola como **ponto de partida conhecido**, não como afirmação de que é a próxima branch a desenvolver. Em cada etapa, checar mudanças pendentes e adaptar os testes de paridade a todos os idiomas realmente implementados. Se o `.exe` não existir, a execução deve parar sem publicar nada. Registrar log/resultado local e versão do SDK sem incluir dados pessoais ou tokens no dossiê público.

## Disponibilizar a prévia no FILEBRIDGE privado

Os comandos a seguir **não foram executados nesta conversa**. Executar apenas com binário local compilado e validado. Usar tag exclusiva por commit; nunca sobrescrever uma release existente ou apresentar uma prévia como versão final:

```powershell
$bridgeRepo = Read-Host 'Informe owner/FILEBRIDGE do repositório privado'
gh repo view $bridgeRepo --json nameWithOwner,isPrivate
if ($LASTEXITCODE -ne 0) { throw 'Sem acesso ao FILEBRIDGE.' }
$tag = 'wins-sidebar-dev-' + $commit.Substring(0,12)
$note = Join-Path (Get-Location) 'preview-notes.txt'
@(
  'PRÉVIA DE DESENVOLVIMENTO — NÃO É RELEASE OFICIAL'
  "Origem: hamthet/WinSidebar@$commit"
  "SHA-256 de WinSidebar.exe: $sha256"
  'Testes interativos e aprovação do responsável ainda devem ser registrados.'
) | Set-Content -Path $note -Encoding utf8

gh release create $tag $exe --repo $bridgeRepo --target main --prerelease --title "WinSidebar preview $($commit.Substring(0,12))" --notes-file $note
if ($LASTEXITCODE -ne 0) { throw 'Upload da prévia falhou; não anunciar download.' }
gh release view $tag --repo $bridgeRepo --json tagName,isPrerelease,assets
if ($LASTEXITCODE -ne 0) { throw 'Prévia não pôde ser confirmada.' }
```

Este procedimento cria uma prerelease/tag no **FILEBRIDGE**, não no WinSidebar. A existência e os detalhes da prerelease precisam ser conferidos após a operação. A distribuição de um arquivo ~72 MB pelo FILEBRIDGE deve respeitar as cotas/regras do GitHub e o acesso privado; não assumir que qualquer pessoa conseguirá baixar. Não armazenar o `.exe` no histórico Git, não publicar token ou URL temporária de download no dossiê e não alterar a release 1.0.0 do WinSidebar.

## Baixar e conferir a prévia no computador de teste

```powershell
$bridgeRepo = Read-Host 'Informe owner/FILEBRIDGE do repositório privado'
$tag = Read-Host 'Informe a tag exata da prévia'
$expected = (Read-Host 'Informe o SHA-256 do EXE publicado').Trim().ToLowerInvariant()
$dest = Join-Path (Get-Location) 'wins-sidebar-preview-download'
New-Item -ItemType Directory -Path $dest -Force | Out-Null
gh release download $tag --repo $bridgeRepo --pattern 'WinSidebar.exe' --dir $dest
if ($LASTEXITCODE -ne 0) { throw 'Download da prévia falhou.' }
$received = (Get-FileHash (Join-Path $dest 'WinSidebar.exe') -Algorithm SHA256).Hash.ToLowerInvariant()
if ($received -ne $expected) { throw "SHA-256 divergente: $received" }
'Hash verificado. Executar somente após conferir origem e segurança do arquivo.'
```

**Aceitação:** registrar versão/commit, Windows 10/11, idioma, escala de exibição, comportamento observado e falhas reproduzíveis; não publicar logs com caminhos privados. O responsável é quem aprovará os cinco idiomas. Registro `PASS` só após resultado real. Distinguir o hash do arquivo EXE do hash de um ZIP que eventualmente o contenha.

## Estado no momento da decisão

- `main` e a release 1.0.0 não foram alteradas por esta decisão; PRs #1, #2 e #3 permanecem rascunhos.
- A prévia espanhola antiga foi gerada **anteriormente** pelo GitHub Actions e expira segundo o GitHub; não é uma entrega via FILEBRIDGE. Não anunciá-la como se tivesse sido copiada.
- Nenhum build local via `gh`/PowerShell, upload no FILEBRIDGE, desativação de workflow ou teste manual foi executado **nesta etapa de documentação**. A cobrança efetiva de Actions permanece não verificada.
