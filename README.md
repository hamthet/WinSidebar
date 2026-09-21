# WinSidebar

![Ilustração conceitual do WinSidebar em um desktop com dois monitores](assets/hero-ilustrativo.svg)

> **Arte meramente ilustrativa, gerada com auxílio de IA.** Não é captura de tela nem representação exata dos controles do aplicativo. A interface inspirada no **Windows 98 é intencional** — uma escolha visual, não uma falha de modernização.

Uma barra lateral retrátil para **encontrar e alternar entre janelas**, organizadas por monitor, com quatro atalhos configuráveis para pastas e sites. Portátil, sem instalador e sem exigir que o usuário baixe o .NET separadamente. **Código aberto sob a [licença MIT](LICENSE).**

**[⬇ Baixar WinSidebar 1.0.0 para Windows 10/11 x64](https://github.com/hamthet/WinSidebar/releases/tag/v1.0.0)** · **[Ler o tutorial passo a passo](docs/TUTORIAL.md)**

> **Aviso sobre assinatura digital:** o executável desta versão **não possui assinatura digital**. O Windows Defender SmartScreen pode exibir um aviso de aplicativo não reconhecido, e políticas de segurança podem impedir sua execução. Baixe apenas da [release oficial](https://github.com/hamthet/WinSidebar/releases/tag/v1.0.0), confira o arquivo `SHA256SUMS.txt` disponível nela se desejar verificar a integridade do download e respeite as políticas de TI do seu computador. **O checksum não substitui uma assinatura digital.**

## Comece em um minuto

1. Na página de lançamento, baixe **`WinSidebar-v1.0.0-win-x64.zip`**, em *Assets*.
2. Extraia o ZIP em uma pasta sua.
3. Abra **`WinSidebar.exe`**. O arquivo `LEIA-ME.txt` contém as instruções essenciais.

O ZIP contém **somente o executável e o LEIA-ME**. Para usar o aplicativo, não são necessários PowerShell, Git, compilador, instalador nem download separado do .NET. O executável inclui o runtime .NET 8.

## O que você pode fazer

- **Alternar janelas:** a aba lateral abre e recolhe a barra; `Shift+F1` alterna a barra, `Shift+F2/F3` navegam entre janelas e `Shift+F4` ativa a janela selecionada.
- **Organizar por monitor:** visualizar as janelas do monitor principal e, quando houver, do secundário.
- **Personalizar quatro atalhos:** na seção **PASTAS / WEB**, clique na engrenagem e escolha nome, pasta ou URL, ícone e navegador. Os padrões genéricos são **Documentos**, **Downloads**, **Acervo ainda não configurado** e **Google**.
- **Ajustar a barra:** no cabeçalho, mude de lado, diminua ou aumente a largura e use o X vermelho para encerrar.

**[Veja o tutorial ilustrado com todas as etapas →](docs/TUTORIAL.md)**

## Privacidade e versões existentes

As preferências ficam em `%LOCALAPPDATA%\WinSidebar`, isoladas das outras instalações. O WinSidebar não substitui executáveis anteriores, não altera o navegador padrão, não ativa a inicialização automática e não envia telemetria. Para evitar conflitos de teclas globais, feche outra edição da barra antes de iniciar esta.

Para remover, encerre pelo X e apague `WinSidebar.exe`. Opcionalmente, apague `%LOCALAPPDATA%\WinSidebar` para excluir **as preferências desta edição**.

## Projeto, licença e divulgação

O código-fonte C# fica em [`src/`](src/); a compilação Windows é verificada pelo [fluxo de build](.github/workflows/build.yml). O projeto é distribuído sob a **[licença MIT](LICENSE)**, que permite usar, modificar e redistribuir o código, inclusive comercialmente, mantendo os avisos de autoria e licença. Você pode abrir uma [issue](https://github.com/hamthet/WinSidebar/issues/new) para relatar erros e sugerir melhorias, evitando incluir dados pessoais em capturas ou logs.

A [arte quadrada ilustrativa para divulgação](assets/linkedin-ilustrativo.svg) e o [texto sugerido para LinkedIn com instruções de publicação](docs/DIVULGACAO.md) estão no repositório. **Nenhuma das artes deve ser apresentada como captura real do aplicativo.**