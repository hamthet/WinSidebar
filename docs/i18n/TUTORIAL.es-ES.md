# Tutorial de WinSidebar 2.0

[README](README.es-ES.md) · [English](../../docs/TUTORIAL.md) · [Português](TUTORIAL.pt-BR.md) · [Русский](TUTORIAL.ru-RU.md) · [简体中文](TUTORIAL.zh-CN.md)

## 1. Inicia la aplicación portátil

Extrae el ZIP de WinSidebar 2.0 y ejecuta WinSidebar.exe. No hace falta instalador ni runtime .NET separado.

En un perfil totalmente nuevo, English aparece preseleccionado. El selector inicial también ofrece Português (Brasil), Español, Русский y 简体中文. La elección se guarda en %LOCALAPPDATA%\WinSidebar.

Si Windows muestra SmartScreen, recuerda que el ejecutable actual no está firmado digitalmente. Sigue la política de seguridad de tu organización.

## 2. Abre y cierra la barra

Puedes hacer clic en la pestaña estrecha del borde de la pantalla o pulsar AltGr+Y.

AltGr+Y alterna ambos estados: cerrada pasa a abierta y abierta pasa a cerrada.

Los controles del encabezado permiten cambiar de lado y recorrer los anchos y altos disponibles. Esas opciones se guardan.

## 3. Cambia entre ventanas

Abre la barra y haz clic en una ventana elegible. WinSidebar agrupa las ventanas por monitor.

Clic derecho sobre una ventana:

- Cambiar nombre — asigna una etiqueta temporal a esa ventana viva;
- Restablecer nombre — elimina la etiqueta temporal;
- Ignorar esta aplicación — oculta de forma persistente las ventanas de esa aplicación.

El menú contextual general permite administrar aplicaciones ignoradas y volver a mostrarlas.

La detección usa metadatos y heurísticas de Windows; algunas aplicaciones pueden exponer ventanas que no coinciden exactamente con Alt+Tab.

## 4. Configura accesos directos

La sección Accesos directos comienza con cuatro elementos.

- Clic izquierdo: abre carpeta o sitio.
- Clic derecho: edita.
- +: añade una fila de cuatro.
- −: quita la última fila añadida, nunca por debajo de cuatro.
- Restaurar: restablece solo los accesos directos.

El máximo es 12.

El editor configura nombre, destino, tipo, navegador e icono. Los iconos personalizados se copian al perfil de WinSidebar.

F1–F4 abren globalmente los accesos directos 1–4. Los demás se accionan con el ratón.

## 5. Configura fragmentos de texto

La sección Scripts almacena texto literal, no código ejecutable.

- Clic en la fila: pega el texto en la última aplicación externa elegible.
- Engranaje: edita nombre, contenido y atajo.
- +: añade un fragmento.
- −: quita el último añadido, nunca por debajo de cuatro.
- Restaurar: restablece solo los fragmentos.

El máximo es 8.

Los fragmentos 1–4 usan Shift+F1–F4 de forma predeterminada. Cada uno puede usar Ninguno o Shift+F1 a Shift+F12. No se permiten duplicados.

WinSidebar usa temporalmente el portapapeles para pegar e intenta restaurarlo después. Windows puede bloquear la entrada simulada hacia aplicaciones elevadas.

## 6. Cambia de idioma

Abre el menú contextual general y selecciona Idioma. Están disponibles English, Português (Brasil), Español, Русский y 简体中文.

La selección se guarda en settings.ini.

Una instalación nueva siempre comienza en inglés, independientemente del idioma de Windows. Los perfiles antiguos creados antes de guardar el idioma pueden seguir inicialmente en portugués.

## 7. Archivos del perfil

WinSidebar guarda el estado en:

    %LOCALAPPDATA%\WinSidebar

La carpeta puede contener settings.ini, shortcuts.xml, snippets.json, ignored-apps.json, icons\ y archivos .bak.

Los datos escritos por el usuario, como nombres, rutas, textos y títulos de ventanas, nunca se traducen.

## 8. Restaurar y recuperar

Restaurar accesos directos y Restaurar scripts son independientes.

Si un archivo del perfil queda ilegible, conserva el original antes de borrarlo o reemplazarlo manualmente. Puede ser útil para recuperar datos.

Si vienes de una versión anterior de WinSidebar, haz primero una copia de este perfil; no se garantiza la migración del perfil entre versiones.

## 9. Desinstalar

Cierra WinSidebar y elimina WinSidebar.exe y la documentación extraída.

El perfil es independiente. Elimina %LOCALAPPDATA%\WinSidebar solo si también quieres borrar la configuración guardada.

## 10. Referencia de teclado

| Comando | Acción |
| --- | --- |
| AltGr+Y | Abrir/cerrar la barra |
| F1–F4 | Abrir accesos directos 1–4 |
| Shift+F1–F4 | Atajos predeterminados de fragmentos 1–4 |
| Shift+F1–F12 | Atajos configurables disponibles |

La antigua navegación de ventanas mediante Shift+F se eliminó intencionalmente en 2.0.