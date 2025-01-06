
# Desarrollo de minijuegos para el recorrido virtual de la Universidad del Valle de Guatemala

Este proyecto es parte de un megaproyecto que tiene el objetivo de desarrollar una aplicación para recorridos virtuales mediante el uso de realidad aumentada para visitas guiadas en el Centro de Innovación y Tecnología de la Universidad del Valle de Guatemala. El módulo en específico se enfoca en el desarrollo de minijuegos inspirados en lugares seleccionados dentro del recorrido. Se desarrollaron tres juegos: Breakout, Trivia y Flappy Bird.


## Instrucciones de instalación

Este proyecto es un proyecto de Unity, por lo que puede ser accedido únicamente a traves del editor de unity.

La version del editor de Unity utilizada para el proyecto es la `2022.3.18f1`. Dicha versión es importante si se quiere seguir trabajando sobre el proyecto actual ya que de esto dependen el resto de librerías dentro del ambiente.

Los paquetes instalados y sus versiones son los siguientes:

- 2D Animation `9.0.4`
- 2D Common \textbf{8.0.2}
- 2D Pixel Perfect \textbf{5.0.3}
- 2D  Sprite \textbf{1.0.0}
- 2D  Tilemap Editor \textbf{1.0.0}
- AI Navigation \textbf{1.1.5}
- Apple ARKit XR Plugin \textbf{5.1.5}
- AR Foundation \textbf{5.1.5}
- Burst \textbf{1.8.12}
- Cinemachine \textbf{2.9.7}
- Collections \textbf{1.2.4}
- Custom NUnit \textbf{1.0.6}
- Editor Coroutines \textbf{1.0.0}
- Google ARCore XR Plugin \textbf{5.1.5}
- Input System \textbf{1.7.0}
- Magic Leap XR Plugin \textbf{7.0.0}
- Mathematics \textbf{1.2.6}
- Newtonsoft Json \textbf{3.2.1}
- OpenXR Plugin \textbf{1.9.1}
- Test Framework \textbf{1.1.33}
- TextMeshPro \textbf{3.0.9}
- Unity UI \textbf{1.0.0}
- Version Control \textbf{2.4.3}
- Visual Studio Editor \textbf{2.0.22}
- XR Core Utilities \textbf{2.2.3}
- XR Interaction Subsystems \textbf{2.0.0}
- XR Plugin Management \textbf{4.5.0}

Cabe resaltar que aunque estas librerías no se utilicen en todas las escenas es primordial tener el ambiente para que todos los componentes funcionen correctamente. Cosas como tecnologías de XR y AR no son necesarias en este módulo pero sí en la integración con el resto de módulos para realizar el tour virtual de la universidad. 

Para realizar pruebas desde dispositivos moviles es importante tener AR Core, para dispositivos Android.

### Build en Unity
Abre tu proyecto en Unity.
Ve a `File > Build Settings.`
Selecciona la plataforma de destino (por ejemplo, Windows, Mac, iOS, Android, etc.).
Si la plataforma no está marcada como activa, haz clic en Switch Platform.

Haz clic en `Build` o `Build and Run`.
Elige una carpeta donde se guardará el build.
Crea una nueva carpeta para evitar sobrescribir archivos importantes.
Espera a que Unity complete el proceso.

## Demo

[Ver video de demostración](demo/Demo.mp4)
