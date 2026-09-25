# Notas de la versión WinSidebar 2.0

[English](../RELEASE-NOTES.md) · [Português](RELEASE-NOTES.pt-BR.md) · [Русский](RELEASE-NOTES.ru-RU.md) · [简体中文](RELEASE-NOTES.zh-CN.md)

La versión 2.0 es una revisión importante del producto.

## Novedades

- Runtime completo en inglés, portugués de Brasil, español, ruso y chino simplificado.
- Inglés es el predeterminado de instalaciones nuevas; la elección se guarda.
- Un único ejecutable .NET 8 autocontenido para Windows x64.
- De 4 a 12 accesos directos configurables con controles independientes.
- De 4 a 8 fragmentos de texto literal con controles independientes.
- Atajos de fragmentos: Ninguno o Shift+F1 a Shift+F12; los cuatro primeros usan Shift+F1–F4 por defecto.
- F1–F4 abren los accesos directos 1–4.
- AltGr+Y abre/cierra la barra.
- Edición de accesos con clic derecho.
- Menú de ventana para renombrado temporal, restablecimiento e ignorar aplicaciones de forma persistente.
- Ancho, alto y lado de la barra persistentes.
- README, tutorial, inicio rápido, guía de publicación y arte conceptual en cinco idiomas.

## Compatibilidad

Se elimina la antigua navegación de ventanas mediante Shift+F. Los perfiles antiguos sin idioma guardado pueden conservar portugués hasta una elección explícita. La migración de preferencias entre versiones queda fuera del alcance de validación de 2.0.

## Seguridad y distribución

El ejecutable es portátil y no está firmado. SmartScreen o las políticas de la organización pueden advertir. WinSidebar no habilita inicio automático, no cambia el navegador predeterminado y no envía telemetría.

Los fragmentos usan temporalmente el portapapeles para pegar e intentan restaurarlo después.