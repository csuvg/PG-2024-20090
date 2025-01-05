using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using System.Collections.Generic;
// using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using Assets.Minijuegos.Scripts;

// Agregar esto justo antes de la clase ControladorVistas
[System.Serializable]
public class PuntoDeInteres
{
    public string name;
    public string description;

    public PuntoDeInteres(string name, string description)
    {
        this.name = name;
        this.description = description;
    }
}

public class ControladorVistas : MonoBehaviour
{

    // Instancia de la clase

    public static ControladorVistas Instance { get; private set; }

    // Seccion de ventanas:
    public UIDocument ventanaPrincipal; 
    public UIDocument ventanaEscanearQR;
    public UIDocument ventanaInformacionRuta;
    public UIDocument ventanaInformacionCompletaRuta;

    // Seccion de elementos root de las ventanas:
    public VisualElement rootVentanaPrincipal;
    public VisualElement rootVentanaEscanearQR;
    public VisualElement rootVentanaInformacionRuta;
    public VisualElement rootVentanaInformacionCompletaRuta;

    // ContenedorDeTriggerVideoJuego que se encuentra dentro de rootVentanaInformacionRuta
    public VisualElement contenedorDeTriggerVideoJuego;


    private bool isVentanaPrincipalVisible;
    private bool isVentanaEscanearQRVisible;
    private bool isVentanaInformacionRutaVisible;
    private bool isVentanaInformacionCompletaRutaVisible;




    // Seccion de botones controladores de eventos:
    private Button botonComenzarRutaExpress;
    private Button botonComenzarRutaCompleta;
    private Button botonAbrirInformacionCompletaRuta;
    private Button botonCerrarInformacionCompletaRuta;
    private Button botonIrAMenuVideoJuegos;

    private Button botonInicioForzado;

    private Button botonDebugSiguienteObjetivo;

    private Button botonIniciarVideoJuego;

    [SerializeField]
    private Assets.Minijuegos.Scripts.Scene scene;






    public VideoPlayer videoPlayer; // Referencia al VideoPlayer desde el Inspector

    public Texture2D imagen_admisiones_recepción;
    public Texture2D imagen_elevadores_2_ubicacion1;
    public Texture2D imagen_marketspace_d_hive;
    public Texture2D imagen_planta_alimentaria;
    public Texture2D imagen_elevadores_1_ubicacion1;
    public Texture2D imagen_finanzas_avanzadas;
    public Texture2D imagen_procesos_industriales;
    public Texture2D imagen_mariposario;
    public Texture2D imagen_elevadores_2_ubicacion2;
    public Texture2D imagen_marketspace_steam;
    public Texture2D imagen_piezas_arqueologicas;
    public Texture2D imagen_biblioteca;
    public Texture2D imagen_elevadores_3_ubicacion1;
    public Texture2D imagen_sala_de_pensamiento_creativo;
    public Texture2D imagen_estudio_de_grabacion;
    public Texture2D imagen_elevadores_4_ubicacion1;
    public Texture2D imagen_cafeteria_cafe;
    public Texture2D imagen_oficina_de_maestrias;
    public Texture2D imagen_elevadores_6_ubicacion1;
    public Texture2D imagen_historia_uvg;
    public Texture2D imagen_terraza;
    public Texture2D imagen_fuente;
    public Texture2D imagen_anfiteatro;
    public Texture2D imagen_elevadores_7_ubicacion1;
    public Texture2D imagen_elevadores_2_ubicacion3;
    public Texture2D imagen_elevadores_3_ubicacion2;
    public Texture2D imagen_elevadores_4_ubicacion2;
    public Texture2D imagen_edificio_c;
    public Texture2D imagen_elevadores_1_ubicacion2;
    
    

    public VideoClip videoClipPiso1;
    public VideoClip videoClipPiso2;
    public VideoClip videoClipPiso3;
    public VideoClip videoClipPiso4;
    public VideoClip videoClipPiso6;
    public VideoClip videoClipPiso7;


    // Referencias a los elementos dinámicos
    private VisualElement objetoDinamicoVideo;
    private VisualElement objetoDinamicoImagen;
    private Label objetoDinamicoTexto;
    private Label objetoDinamicoTextoInformacion;

    private VisualElement objetoDinamicoImagenPreviewVentanaInformacionRuta;


    // Para texto_dinamico_objetivo_actual es una string que se actualiza con el nombre del objetivo actual
    private string texto_dinamico_objetivo_actual = "";
    // Para texto_dinamico_nivel_actual es una string que se actualiza con el nivel actual
    private string texto_dinamico_nivel_actual = "";
    // Para texto_dinamico_recorrido_actual es una string que se actualiza con el recorrido actual
    private string texto_dinamico_recorrido_actual = "Recorrido 1";
    // Para texto_dinamico_distancia_a_objetivo es una string que se actualiza con la distancia al objetivo
    private string texto_dinamico_distancia_a_objetivo = "0.0 m";
    // Para texto_dinamico_porcentaje_recorrido es una string que se actualiza con el porcentaje de recorrido
    private string texto_dinamico_porcentaje_recorrido = "0%";

    private Dictionary<string, PuntoDeInteres> informacionRutas;

    private void Awake()
    {
        // Asegurarse de que solo haya una instancia de SetNavigationTarget
        if (Instance == null)
        {
            Instance = this;
            rootVentanaInformacionRuta = ventanaInformacionRuta.rootVisualElement;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Destruir el objeto si ya existe una instancia
        }
    }

    public void UpdateTextoObjetivoActual(string texto)
    {
        Label label = rootVentanaInformacionRuta.Q<Label>("texto_dinamico_objetivo_actual");
        if (label != null)
        {
            label.text = texto;
            texto_dinamico_objetivo_actual = texto; // Actualizar la variable privada
        }

        // Actualizar objetoDinamicoTexto directamente
        if (objetoDinamicoTexto != null)
        {
            objetoDinamicoTexto.text = texto;
        }

        // Actualizar el contenido dinámico
        UpdateContenidoDinamico(texto);
    }

    public void UpdateTextoDistanciaObjetivo(string texto)
    {
        Label label = rootVentanaInformacionRuta.Q<Label>("texto_dinamico_distancia_a_objetivo");
        if (label != null)
        {
            label.text = texto;
        }
    }

    public void UpdateTextoPorcentajeRecorrido(string texto)
    {
        Label label = rootVentanaInformacionRuta.Q<Label>("texto_dinamico_porcentaje_recorrido");
        if (label != null)
        {
            label.text = texto;
        }
    }

    public void UpdateTextoNivelActual(string texto)
    {
        Label label = rootVentanaInformacionRuta.Q<Label>("texto_dinamico_nivel_actual");
        if (label != null)
        {
            label.text = texto;
            texto_dinamico_nivel_actual = texto; // Actualizar la variable privada
        }
    }



    public void UpdateTexto_jack_en_escena_qr(string texto){
        Label label = rootVentanaEscanearQR.Q<Label>("texto_jack_en_escena_qr");
        if (label != null)
        {
            label.text = texto;
        }
    }






    public void mostrarVentanaEscanearQR(){
        rootVentanaEscanearQR.style.display = DisplayStyle.Flex;
    }
    public void esconderVentanaEscanearQR(){
        rootVentanaEscanearQR.style.display = DisplayStyle.None;
    }

    public void mostrarVentanaInformacionRuta(){
        rootVentanaInformacionRuta.style.display = DisplayStyle.Flex;
    }
    public void esconderVentanaInformacionRuta(){
        rootVentanaInformacionRuta.style.display = DisplayStyle.None;
    }
    public void desactivarBotonDebugSiguienteObjetivo(){
        rootVentanaInformacionRuta.Q<Button>("boton_debug_siguiente_objetivo").style.display = DisplayStyle.None;
    }
    public void activarBotonDebugSiguienteObjetivo(){
        rootVentanaInformacionRuta.Q<Button>("boton_debug_siguiente_objetivo").style.display = DisplayStyle.Flex;
    }
    public void mostrarContenedorDeTriggerVideoJuego(){
        contenedorDeTriggerVideoJuego.style.display = DisplayStyle.Flex;
    }
    public void esconderContenedorDeTriggerVideoJuego(){
        contenedorDeTriggerVideoJuego.style.display = DisplayStyle.None;
    }










    // Start is called before the first frame update
    void Start()
    {
        // Obtener el root visual de ambas ventanas
        rootVentanaPrincipal = ventanaPrincipal.rootVisualElement;
        rootVentanaEscanearQR = ventanaEscanearQR.rootVisualElement;
        rootVentanaInformacionCompletaRuta = ventanaInformacionCompletaRuta.rootVisualElement;


        botonComenzarRutaExpress = rootVentanaPrincipal.Q<Button>("boton_ruta_express");
        botonComenzarRutaCompleta = rootVentanaPrincipal.Q<Button>("boton_ruta_completa");
        // Obtenemos de la vista rootVentanaInformacionRuta el boton boton_mostrar_informacion 
        botonAbrirInformacionCompletaRuta = rootVentanaInformacionRuta.Q<Button>("boton_mostrar_informacion");
        // Obtenemos de la vista rootVentanaInformacionCompletaRuta el boton boton_regresar_a_recorrido
        botonCerrarInformacionCompletaRuta = rootVentanaInformacionCompletaRuta.Q<Button>("boton_regresar_a_recorrido");
        botonDebugSiguienteObjetivo = rootVentanaInformacionRuta.Q<Button>("boton_debug_siguiente_objetivo");
        botonIrAMenuVideoJuegos = rootVentanaPrincipal.Q<Button>("boton_ir_a_videojuegos");
        botonInicioForzado = rootVentanaEscanearQR.Q<Button>("inicio_forzado");
        botonIniciarVideoJuego = rootVentanaInformacionRuta.Q<Button>("boton_activar_videojuego");



        // Obtenemos texto_dinamico_objetivo_actual, texto_dinamico_nivel_actual, texto_dinamico_recorrido_actual, texto_dinamico_distancia_a_objetivo y texto_dinamico_porcentaje_recorrido de la vista rootVentanaInformacionRuta
        texto_dinamico_objetivo_actual = rootVentanaInformacionRuta.Q<Label>("texto_dinamico_objetivo_actual").text;
        texto_dinamico_nivel_actual = rootVentanaInformacionRuta.Q<Label>("texto_dinamico_nivel_actual").text;
        texto_dinamico_recorrido_actual = rootVentanaInformacionRuta.Q<Label>("texto_dinamico_recorrido_actual").text;
        texto_dinamico_distancia_a_objetivo = rootVentanaInformacionRuta.Q<Label>("texto_dinamico_distancia_a_objetivo").text;
        texto_dinamico_porcentaje_recorrido = rootVentanaInformacionRuta.Q<Label>("texto_dinamico_porcentaje_recorrido").text;

        // Obtenemos ImagenActual de rootVentanaInformacionRuta que es la imagen preview del punto actual
        objetoDinamicoImagenPreviewVentanaInformacionRuta = rootVentanaInformacionRuta.Q<VisualElement>("ImagenActual");

        // Obtener las referencias de los elementos dinámicos en rootVentanaInformacionCompletaRuta
        objetoDinamicoVideo = rootVentanaInformacionCompletaRuta.Q<VisualElement>("objeto_dinamico_video_punto_actual");
        objetoDinamicoImagen = rootVentanaInformacionCompletaRuta.Q<VisualElement>("objeto_dinamico_imagen_punto_actual");
        objetoDinamicoTexto = rootVentanaInformacionCompletaRuta.Q<Label>("objeto_dinamico_texto_punto_actual");
        objetoDinamicoTextoInformacion = rootVentanaInformacionCompletaRuta.Q<Label>("objeto_dinamico_texto_informacion_punto_actual");



        // // Asegurarnos de que todas las demas ventanas esten ocultas excepto la principal
        rootVentanaPrincipal.style.display = DisplayStyle.Flex;
        rootVentanaEscanearQR.style.display = DisplayStyle.None;
        rootVentanaInformacionRuta.style.display = DisplayStyle.None;
        rootVentanaInformacionCompletaRuta.style.display = DisplayStyle.None;

        // Obtener el contenedor de trigger de video juego
        contenedorDeTriggerVideoJuego = rootVentanaInformacionRuta.Q<VisualElement>("ContenedorDeTriggerVideoJuego");

        // Lo ocultamos al inicio
        contenedorDeTriggerVideoJuego.style.display = DisplayStyle.None;

        botonInicioForzado.clicked += OnClickInicioForzado;

        // // Asignar el evento al botón de ir a menú de videojuegos
        botonIrAMenuVideoJuegos.clicked += OnClickIrAMenuVideoJuegos;

        // // Asignar el evento al botón de la ruta express
        botonComenzarRutaExpress.clicked += OnRutaExpressClicked;

        // Asignar el evento al botón de ruta completa
        botonComenzarRutaCompleta.clicked += onRutaCompletaClicked;

        // Asignar el evento al botón de mostrar información completa de la ruta
        botonAbrirInformacionCompletaRuta.clicked += OnMostrarInformacionCompletaRutaClicked;

        // Asignar el evento al botón de cerrar información completa de la ruta
        botonCerrarInformacionCompletaRuta.clicked += OnCerrarInformacionCompletaRutaClicked;

        // Asignar el evento al botón de debug siguiente objetivo
        botonDebugSiguienteObjetivo.clicked += OnDebugSiguienteObjetivoClicked;

        // Asignar el evento al botón de iniciar video juego
        botonIniciarVideoJuego.clicked += OnClickIniciarVideoJuego;

        objetoDinamicoTexto = rootVentanaInformacionCompletaRuta.Q<Label>("objeto_dinamico_texto_punto_actual");
        
        // Asegúrate de que objetoDinamicoTexto no sea null
        if (objetoDinamicoTexto == null)
        {
            Debug.LogError("objetoDinamicoTexto no se pudo inicializar correctamente.");
        }

        InicializarInformacionRutas();

        // // Ejemplo de cómo acceder a la información del punto 20
        // PuntoDeInteres punto20 = ObtenerInformacionPunto("20");
        // if (punto20 != null)
        // {
        //     Debug.Log($"Nombre del punto 20: {punto20.name}");
        //     Debug.Log($"Descripción del punto 20: {punto20.description}");
        // }
        // else
        // {
        //     Debug.Log("No se encontró información para el punto 20");
        // }
    }

    public void GuardarEstadoVistas()
    {
        isVentanaPrincipalVisible = rootVentanaPrincipal.style.display == DisplayStyle.Flex;
        isVentanaEscanearQRVisible = rootVentanaEscanearQR.style.display == DisplayStyle.Flex;
        isVentanaInformacionRutaVisible = rootVentanaInformacionRuta.style.display == DisplayStyle.Flex;
        isVentanaInformacionCompletaRutaVisible = rootVentanaInformacionCompletaRuta.style.display == DisplayStyle.Flex;
    }

    public void RestaurarEstadoVistas()
    {
        rootVentanaPrincipal.style.display = isVentanaPrincipalVisible ? DisplayStyle.Flex : DisplayStyle.None;
        rootVentanaEscanearQR.style.display = isVentanaEscanearQRVisible ? DisplayStyle.Flex : DisplayStyle.None;
        rootVentanaInformacionRuta.style.display = isVentanaInformacionRutaVisible ? DisplayStyle.Flex : DisplayStyle.None;
        rootVentanaInformacionCompletaRuta.style.display = isVentanaInformacionCompletaRutaVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void InicializarInformacionRutas()
    {
        informacionRutas = new Dictionary<string, PuntoDeInteres>
        {
            {"1", new PuntoDeInteres("Admisiones y Recepción", "Aquí encontrarás toda la información necesaria en cuanto al proceso de admisión y de todas las carreras que ofrecemos. Puedes acercarte para poder resolver tus dudas.")},
            {"2", new PuntoDeInteres("Elevadores 2", "Porfavor, dirigete a los elevadores para poder cambiar de nivel.")},
            {"3", new PuntoDeInteres("Marketspace D-HIVE", "El makerspace D-Hive es un espacio colaborativo exclusivo para poder crear, aprender, explorar y compartir utilizando herramientas manuales y de alta tecnología. Su objetivo es proporcionar a estudiantes, docentes y personal administrativo los recursos necesarios para materializar sus ideas, destacando el diseño y desarrollo. Es conocido “D” de “diseño y desarrollo”, y “Hive”, de colmena en español, que hace referencia al trabajo de la comunidad UVG.")},
            {"4", new PuntoDeInteres("Planta Alimentaria", "La Planta de Innovación Alimentaria y Nutricional, es la más grande y completa de Guatemala, con más de 500 equipos a la disposición de la comunidad UVG. Su objetivo es poder comprender los procesos alimenticios desde su formulación hasta su desarrollo, con la capacidad de producir productos a nivel piloto, para desarrollarlos a escala macro.")},
            {"5", new PuntoDeInteres("Elevadores 1", "Por favor, dirigete a los elevadores para poder cambiar de nivel.")},
            {"6", new PuntoDeInteres("Finanzas Avanzadas", "El laboratorio de Finanzas Avanzadas es el primero en toda Guatemala, siendo un espacio en donde los estudiantes pueden aprender de forma activa y observar y analizar fenómenos económicos y financieros de Guatemala y el Mundo. Esto representa mayor incorporación al mundo de los mercados más grandes del mundo, como New York Exchange Commission.")},
            {"7", new PuntoDeInteres("Procesos Industriales", "El laboratorio de Procesos Industriales y Realidad Virtual es un espacio diseñado para desarrollar procesos desde las ideas hasta el modelado y simulación. Su objetivo es generar conocimiento que aporte valor a la sociedad y formar profesionales capaces de ofrecer soluciones creativas e innovadoras.")},
            {"8", new PuntoDeInteres("Mariposario", "Información sobre este punto de interés no encontrada en el PDF.")},
            {"9", new PuntoDeInteres("Elevadores 2", "Por favor, dirigete a los elevadores para poder cambiar de nivel.")},
            {"10", new PuntoDeInteres("Marketspace Steam", "Steam Marketplace es un espacio que engloba la tecnología, ingeniería, arte y las matemáticas, brindando una comunidad de aprendizaje colaborativo, creando y diseñando soluciones creativas para retos del mundo actual.")},
            {"11", new PuntoDeInteres("Piezas Arqueológicas", "La exposición de piezas arqueológicas muestra diversas piezas como parte de una colección del Centro de Investigaciones de Arqueología y Antropología UVG, representando tres períodos de la historia Maya.")},
            {"12", new PuntoDeInteres("Biblioteca", "La biblioteca Amparo Codina de Campollo ofrece recursos de información en distintos formatos para apoyar al aprendizaje orientado a las tecnologías de la información.")},
            {"13", new PuntoDeInteres("Elevadores 3", "Por favor, dirigete a los elevadores para poder cambiar de nivel.")},
            {"14", new PuntoDeInteres("Sala de Pensamiento Creativo", "Es un espacio dedicado a compartir conocimientos y experiencias que permite a los estudiantes desenvolverse mejor, por medio de un lugar cómodo que rete las mentes.")},
            {"15", new PuntoDeInteres("Estudio de Grabación", "Es un estudio de alto nivel con áreas dedicadas al desarrollo de productos multimedia en diferentes formatos, abierto también a usuarios externos que deseen producir materiales.")},
            {"16", new PuntoDeInteres("Elevadores 4", "Por favor, dirigete a los elevadores para poder cambiar de nivel.")},
            {"17", new PuntoDeInteres("Cafetería &café", "La cafetería de la Universidad es un espacio cómodo para disfrutar de comidas saludables y compartir con amigos.")},
            {"18", new PuntoDeInteres("Oficina de Maestrías", "Información sobre este punto de interés no encontrada en el PDF.")},
            {"19", new PuntoDeInteres("Elevadores 6", "Por favor, dirigete a los elevadores para poder cambiar de nivel.")},
            {"20", new PuntoDeInteres("Historia UVG", "Información sobre este punto de interés no encontrada en el PDF.")},
            {"21", new PuntoDeInteres("Terraza", "La terraza del edificio ofrece vistas espectaculares de la Ciudad de Guatemala, creando un ambiente cómodo para disfrutar de la belleza del país.")},
            {"22", new PuntoDeInteres("Fuente", "Información sobre este punto de interés no encontrada en el PDF.")},
            {"23", new PuntoDeInteres("Anfiteatro", "El auditorio de la universidad es un espacio cómodo para realizar actividades como conciertos, obras de teatro, conferencias y debates, con tecnología avanzada.")},
            {"24", new PuntoDeInteres("Elevadores 7", "Por favor, dirigete a los elevadores para poder cambiar de nivel.")},
            {"25", new PuntoDeInteres("Elevadores 2", "Por favor, dirigete a los elevadores para poder cambiar de nivel.")},
            {"26", new PuntoDeInteres("Elevadores 3", "Por favor, dirigete a los elevadores para poder cambiar de nivel.")},
            {"27", new PuntoDeInteres("Elevadores 4", "Por favor, dirigete a los elevadores para poder cambiar de nivel.")},
            {"28", new PuntoDeInteres("Edificio C", "Los laboratorios de química y física en el Edificio C ofrecen espacios diseñados específicamente para experimentos y prácticas relacionadas con la química y los principios físicos.")},
            {"29", new PuntoDeInteres("Elevadores 1", "Por favor, dirigete a los elevadores para poder cambiar de nivel.")}
        };
    }

    public void OnClickIniciarVideoJuego(){
        Debug.Log("Botón de Iniciar Video Juego presionado");
        int idTargetActual = SetNavigationTarget.Instance.GetCurrentTargetId();

        if (idTargetActual == 1){
            scene.LoadScene("Breakout");
            //CambiadorDeScenes.Instance.LoadMinigame("Breakout");
        }
        else if (idTargetActual == 12){
            scene.LoadScene("FlappyBird");
            //CambiadorDeScenes.Instance.LoadMinigame("FlappyBird");
        }
        else if (idTargetActual == 22){
            scene.LoadScene("Trivia");
            //CambiadorDeScenes.Instance.LoadMinigame("Trivia");
        }
    }

    public void OnClickInicioForzado(){
        rootVentanaEscanearQR.style.display = DisplayStyle.None;
        QrCodeRecenter.Instance.ChangeActiveFloor(2);
    }

    public void OnClickIrAMenuVideoJuegos(){
        //CambiadorDeScenes.Instance.LoadMinigame("GameMenu");
        scene.LoadMenu();
        SceneManager.LoadScene("GameMenu");
        Destroy(gameObject);
    }

    public PuntoDeInteres ObtenerInformacionPunto(string indice)
    {
        if (informacionRutas != null && informacionRutas.ContainsKey(indice))
        {
            return informacionRutas[indice];
        }
        return null;
    }

    void OnDebugSiguienteObjetivoClicked()
    {
        Debug.Log("Botón de Debug Siguiente Objetivo presionado");
        // Llamar a la función de SetNavigationTarget para avanzar al siguiente objetivo
        SetNavigationTarget.Instance.AdvanceToNextTarget();
    }

    void onRutaCompletaClicked(){
        Debug.Log("Botón de Ruta Completa presionado");
        // Ocultar VentanaSeleccionRuta
        rootVentanaPrincipal.style.display = DisplayStyle.None;

        // Mostrar VentanaEscanearQR
        rootVentanaEscanearQR.style.display = DisplayStyle.Flex;

        // Iniciar la busqueda del QR
        SetNavigationTarget.Instance.InitializeTour(0);
    }

    // Método que se ejecuta cuando el botón de la ruta express es presionado
    void OnRutaExpressClicked()
    {
        Debug.Log("Botón de Ruta Express presionado");
        // Ocultar VentanaSeleccionRuta
        rootVentanaPrincipal.style.display = DisplayStyle.None;

        // Mostrar VentanaEscanearQR
        rootVentanaEscanearQR.style.display = DisplayStyle.Flex;

        // Iniciar la busqueda del QR
        SetNavigationTarget.Instance.InitializeTour(1);
    }

    void OnMostrarInformacionCompletaRutaClicked()
    {
        Debug.Log("Botón de Mostrar Información Completa de la Ruta presionado");
        // Ocultar VentanaInformacionRuta
        rootVentanaInformacionRuta.style.display = DisplayStyle.None;

        // Mostrar VentanaInformacionCompletaRuta
        rootVentanaInformacionCompletaRuta.style.display = DisplayStyle.Flex;
    }

    void OnCerrarInformacionCompletaRutaClicked()
    {
        Debug.Log("Botón de Cerrar Información Completa de la Ruta presionado");
        // Ocultar VentanaInformacionCompletaRuta
        rootVentanaInformacionCompletaRuta.style.display = DisplayStyle.None;

        // Mostrar VentanaInformacionRuta
        rootVentanaInformacionRuta.style.display = DisplayStyle.Flex;
    }

    public void UpdateContenidoDinamico(string clave)
    {
        // Obtener el número del piso del texto actualizado
        string[] partes = texto_dinamico_nivel_actual.Split(' ');
        string piso = partes.Length > 1 ? partes[1] : "1"; // Por defecto, usar piso 1 si no se puede obtener

        Debug.Log("Actualizando contenido dinamico del piso: " + piso);

        // Segun el piso, asignamos el videoClip correspondiente
        switch (piso)
        {
            case "1":
                Debug.Log("Actualizando contenido dinamico del piso 1");
                videoPlayer.clip = videoClipPiso1;
                break;
            case "2":
                Debug.Log("Actualizando contenido dinamico del piso 2");
                videoPlayer.clip = videoClipPiso2;
                break;
            case "3":
                Debug.Log("Actualizando contenido dinamico del piso 3");
                videoPlayer.clip = videoClipPiso3;
                break;
            case "4":
                Debug.Log("Actualizando contenido dinamico del piso 4");
                videoPlayer.clip = videoClipPiso4;
                break;
            case "6":
                Debug.Log("Actualizando contenido dinamico del piso 6");
                videoPlayer.clip = videoClipPiso6;
                break;
            case "7":
                Debug.Log("Actualizando contenido dinamico del piso 7");
                videoPlayer.clip = videoClipPiso7;
                break;
            default:
                Debug.LogError("Piso no válido");
                videoPlayer.clip = videoClipPiso1;
                break;
        }

        // Actualizar el texto de la ubicacion actual que tiene que ser el mismo que UpdateTextoObjetivoActual mapeado a objetoDinamicoTexto

        objetoDinamicoTexto.text = texto_dinamico_objetivo_actual;

        // Obtener el id del target actual
        int idTargetActual = SetNavigationTarget.Instance.GetCurrentTargetId();
        PuntoDeInteres puntoActual = ObtenerInformacionPunto(idTargetActual.ToString());
        if (puntoActual != null)
        {
            objetoDinamicoTextoInformacion.text = puntoActual.description;
        }
        else
        {
            Debug.LogError("No se encontró información para el punto actual");
            objetoDinamicoTextoInformacion.text = "No se encontró información para el punto actual";
        }

        // Actualizar la imagen del punto actual tomando el idTargetActual con el diccionario para hacer la relacion con la imagen correspondiente

        if (idTargetActual == 1){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_admisiones_recepción);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_admisiones_recepción);
        }
        else if (idTargetActual == 2){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_elevadores_2_ubicacion1);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_elevadores_2_ubicacion1);
        }
        else if (idTargetActual == 3){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_marketspace_d_hive);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_marketspace_d_hive);
        }
        else if (idTargetActual == 4){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_planta_alimentaria);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_planta_alimentaria);
        }
        else if (idTargetActual == 5){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_elevadores_1_ubicacion1);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_elevadores_1_ubicacion1);
        }
        else if (idTargetActual == 6){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_finanzas_avanzadas);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_finanzas_avanzadas);
        }
        else if (idTargetActual == 7){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_procesos_industriales);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_procesos_industriales);
        }
        else if (idTargetActual == 8){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_mariposario);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_mariposario);
        }
        else if (idTargetActual == 9){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_elevadores_2_ubicacion2);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_elevadores_2_ubicacion2);
        }
        else if (idTargetActual == 10){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_marketspace_steam);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_marketspace_steam);
        }
        else if (idTargetActual == 11){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_piezas_arqueologicas);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_piezas_arqueologicas);
        }       
        else if (idTargetActual == 12){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_biblioteca);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_biblioteca);
        }
        else if (idTargetActual == 13){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_elevadores_3_ubicacion1);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_elevadores_3_ubicacion1);
        }
        else if (idTargetActual == 14){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_sala_de_pensamiento_creativo);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_sala_de_pensamiento_creativo);
        }
        else if (idTargetActual == 15){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_estudio_de_grabacion);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_estudio_de_grabacion);
        }
        else if (idTargetActual == 16){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_elevadores_4_ubicacion1);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_elevadores_4_ubicacion1);
        }
        else if (idTargetActual == 17){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_cafeteria_cafe);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_cafeteria_cafe);
        }
        else if (idTargetActual == 18){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_oficina_de_maestrias);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_oficina_de_maestrias);
        }
        else if (idTargetActual == 19){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_elevadores_6_ubicacion1);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_elevadores_6_ubicacion1);
        }
        else if (idTargetActual == 20){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_historia_uvg);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_historia_uvg);
        }
        else if (idTargetActual == 21){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_terraza);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_terraza);
        }
        else if (idTargetActual == 22){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_fuente);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_fuente);
        }
        else if (idTargetActual == 23){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_anfiteatro);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_anfiteatro);
        }
        else if (idTargetActual == 24){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_elevadores_7_ubicacion1);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_elevadores_7_ubicacion1);
        }
        else if (idTargetActual == 25){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_elevadores_2_ubicacion1);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_elevadores_2_ubicacion1);
        }
        else if (idTargetActual == 26){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_elevadores_3_ubicacion1);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_elevadores_3_ubicacion1);
        }
        else if (idTargetActual == 27){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_elevadores_4_ubicacion1);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_elevadores_4_ubicacion1);
        }
        else if (idTargetActual == 28){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_edificio_c);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_edificio_c);
        }
        else if (idTargetActual == 29){
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_elevadores_1_ubicacion1);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_elevadores_1_ubicacion1);
        }    
        else{
            Debug.LogError("No se encontró imagen para el punto actual");
            objetoDinamicoImagenPreviewVentanaInformacionRuta.style.backgroundImage = new StyleBackground(imagen_admisiones_recepción);
            objetoDinamicoImagen.style.backgroundImage = new StyleBackground(imagen_admisiones_recepción);
        }
    }
}