# Fluxo de desenvolvimento local — WinSidebar / FILEBRIDGE

**Decisão do responsável, 2026-09-22 (substitui o procedimento anterior).** Tudo que constitui o projeto — código-fonte, recursos de idioma, testes, documentação, planos e histórico de decisões — pertence a `hamthet/WinSidebar`. O dossiê interno permanece **somente** em `develop`; nenhuma instrução interna entra na `main` final ou no ZIP do produto. `FILEBRIDGE` é o equivalente a `%TEMP%`: **somente transferência temporária de um arquivo que o responsável expressamente precise baixar e testar**. Não é um repositório de desenvolvimento, arquivo permanente, backup, canal genérico de builds ou destino automático de prévias.

## Créditos e limites

O saldo particular do GitHub Actions **não está disponível pelo conector utilizado neste chat**. O titular pode conferir em GitHub > Settings > Billing and licensing > Usage e via `gh api 'users/hamthet/settings/billing/usage?year=2026&month=9&product=Actions'` (consumo faturado, **não** necessariamente saldo remanescente). A documentação do GitHub informa que minutos de runners hospedados padrão em repositórios públicos são gratuitos, mas isso não comprova disponibilidade da conta nem cobre outros custos, como armazenamento ou runners especiais. Respeitar a opção do responsável de **não usar Actions** até nova instrução. Resultados históricos continuam históricos, não testes novos.

O assistente **não tem acesso ao PowerShell do computador do responsável** nem a um compilador .NET/WinForms Windows nesta sessão. Alterações feitas pelo conector GitHub devem ser identificadas como alterações remotas, não como comandos `gh`, builds locais ou testes realizados. Não publicar link ou checksum de um arquivo que não foi realmente enviado e confirmado.

## Desenvolvimento e testes: somente WinSidebar

Execute no PowerShell do Windows. Convenções locais vigentes do responsável: clone em `C:\\Git\\WinSidebar`, SDK portátil .NET 8 em `C:\\dotnet8\\dotnet.exe` e temporários locais em `C:\\H\\filebridge\\WinSidebar`. O diretório local `C:\\H\\filebridge` **não é** o repositório GitHub privado FILEBRIDGE.

```powershell
$ErrorActionPreference = 'Stop'
Set-Location 'C:\Git\WinSidebar'
$dotnet = 'C:\dotnet8\dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet)) { throw 'SDK .NET 8 portátil não encontrado.' }

git fetch origin --prune
git status --short
git rev-parse HEAD

& $dotnet run --project .\tests\LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Teste local de idiomas falhou.' }

$previewRoot = 'C:\H\filebridge\WinSidebar\preview'
New-Item -ItemType Directory -Path $previewRoot -Force | Out-Null
$preview = Join-Path $previewRoot ('WinSidebar-owner-preview-' + [Guid]::NewGuid().ToString('N'))

& $dotnet publish .\WinSidebar.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $preview
if ($LASTEXITCODE -ne 0) { throw 'Build local falhou.' }

$files = @(Get-ChildItem -LiteralPath $preview -File)
if ($files.Count -ne 1 -or $files[0].Name -ne 'WinSidebar.exe') {
    throw 'A prévia deve produzir exatamente um WinSidebar.exe autocontido.'
}
(Get-FileHash -LiteralPath $files[0].FullName -Algorithm SHA256).Hash
```

Antes de qualquer `git push` de código, confirmar que **nenhum workflow automático da branch de destino será acionado**. `gh workflow list --all --repo hamthet/WinSidebar` ajuda a inspecionar, mas não desabilita nada. Em branches de desenvolvimento sem Actions, remover gatilhos `push`/`pull_request` dos arquivos de workflow **antes** de alterar `src/**` ou o `.csproj`, ou, se autorizado e tecnicamente viável, desabilitar os workflows relevantes no próprio repositório via `gh workflow disable <id> --repo hamthet/WinSidebar` e confirmar. Não afirmar que um workflow foi desativado sem resultado verificável. Nunca modificar automaticamente a configuração de Actions na `main` por causa de uma branch experimental.

`gh` gerencia GitHub, `git` controla o repositório, e `dotnet` compila/testa. Nenhuma dessas ferramentas substitui o teste interativo de Alt+Tab, Calculadora, aliases, ignorados, salvar/restaurar, DPI ou dos cinco idiomas pelo responsável. Migração entre versões: `OUT OF SCOPE` por decisão dele.

## FILEBRIDGE: transferência sob demanda, não persistência

**Somente após pedido explícito do responsável por um arquivo para testar**, compilar localmente e conferir o hash. Caso seja necessário usar o FILEBRIDGE privado, transferir **apenas** o executável ou ZIP de teste com identificação de commit e SHA-256. Não copiar código-fonte, planos, scripts de engenharia, histórico, diretórios `development/**` nem versões arquivadas do produto para o FILEBRIDGE. Não cadastrar prévias preventivamente, nem confundir entrega privada com release oficial. O arquivo local de prévia fica em `C:\\H\\filebridge\\WinSidebar`; o arquivo no repositório GitHub privado FILEBRIDGE deve ser excluído quando deixar de ser necessário, após confirmar o download e a integridade. Uma tag/release temporária eventualmente usada para transportar o arquivo também deve ser removida, sem afetar as tags/releases oficiais de `hamthet/WinSidebar`.

Com um executável que de fato exista, exemplo **não executado** de transporte privado por GitHub CLI (o usuário precisa ter acesso à conta privada):

```powershell
$bridge = Read-Host 'owner/FILEBRIDGE privado'
$commit = (git rev-parse HEAD).Trim()
$exe = Join-Path $env:TEMP 'WinSidebar-owner-preview\WinSidebar.exe'
if (-not (Test-Path $exe)) { throw 'Compile e valide antes de transportar.' }
$hash = (Get-FileHash $exe -Algorithm SHA256).Hash.ToLowerInvariant()
$tag = "temporary-wins-sidebar-$($commit.Substring(0,12))"
gh repo view $bridge --json nameWithOwner,isPrivate
if ($LASTEXITCODE -ne 0) { throw 'FILEBRIDGE inacessível.' }
gh release create $tag $exe --repo $bridge --target main --prerelease --latest=false --title "Temporary owner test $($commit.Substring(0,12))" --notes "WinSidebar source commit $commit; WinSidebar.exe SHA256 $hash; TEMPORARY, NOT A PRODUCT RELEASE"
if ($LASTEXITCODE -ne 0) { throw 'Falha no transporte.' }
gh release view $tag --repo $bridge --json tagName,assets,isPrerelease
# No Windows de teste: gh release download $tag --repo $bridge --pattern WinSidebar.exe --dir (Join-Path $env:TEMP 'WinSidebar-owner-test')
# Conferir o SHA256 baixado com o valor informado acima antes de executar.
# Após o teste: gh release delete $tag --repo $bridge --cleanup-tag --yes
```

Uma conta sem acesso ao repositório privado não conseguirá baixar a prévia. Nunca divulgar tokens ou URLs temporárias autenticadas. **Nenhuma transferência para o FILEBRIDGE foi feita por este documento.** Nenhum saldo, execução de PowerShell, build local ou teste visual foi confirmado aqui; registrar cada resultado executado no dossiê `development/**` do WinSidebar.
