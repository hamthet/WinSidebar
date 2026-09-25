# WinSidebar 2.0 — notas de publicación

[README](README.es-ES.md) · [English](../../docs/OUTREACH.md) · [Português](OUTREACH.pt-BR.md) · [Русский](OUTREACH.ru-RU.md) · [简体中文](OUTREACH.zh-CN.md)

## Texto breve sugerido

WinSidebar 2.0 es una barra lateral portátil para Windows 10/11 x64 que permite cambiar entre ventanas, abrir hasta 12 accesos directos y pegar hasta 8 fragmentos de texto reutilizables. Se distribuye como un único ejecutable autocontenido, admite inglés, portugués de Brasil, español, ruso y chino simplificado, y usa inglés como idioma predeterminado de un perfil nuevo.

Controles principales: AltGr+Y abre/cierra la barra, F1–F4 abren los accesos directos 1–4 y Shift+F1–F4 son los atajos predeterminados de los fragmentos.

## Recursos visuales

Usa ../../assets/i18n/es-ES/linkedin-illustration.svg para redes sociales y hero-illustration.svg para presentación.

Son ilustraciones conceptuales, no capturas de pantalla. Conserva esa aclaración al exportarlas o recortarlas.

## Afirmaciones que deben seguir siendo precisas

- Portátil y autocontenido no significa firmado digitalmente.
- El runtime .NET 8 está incluido.
- Hay cinco idiomas de runtime; inglés es el predeterminado de un perfil nuevo.
- Los fragmentos pegan texto literal; no ejecutan scripts.
- La detección de ventanas usa heurísticas; no prometas paridad exacta con Alt+Tab.
- WinSidebar no envía telemetría, no cambia el navegador predeterminado ni habilita inicio automático.

Fuente oficial: https://github.com/hamthet/WinSidebar