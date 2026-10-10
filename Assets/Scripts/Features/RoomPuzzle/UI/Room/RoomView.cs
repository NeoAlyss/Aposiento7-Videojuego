using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UniversalPlatform.Features.RoomPuzzle.Domain;
using UniversalPlatform.Features.RoomPuzzle.Presentation.Room;
using UniversalPlatform.Shared;

namespace UniversalPlatform.Features.RoomPuzzle.UI.Room
{
    /// <summary>
    /// Vista de un aposento. Dibuja el suelo de baldosas visto desde arriba, el protagonista, los
    /// invitados, las llaves y la puerta a partir de la definición de la sala, y traduce el input
    /// (flechas/WASD, clic en una baldosa, Z, R, Escape) a llamadas al ViewModel. No contiene reglas.
    ///
    /// Todo lo que hay sobre el tablero se crea al iniciar, así que una sala nueva solo necesita su
    /// definición en SalasDataRepository; los sprites se cambian en el inspector.
    /// </summary>
    public class RoomView : MonoBehaviour
    {
        /// <summary>Dibujo y tamaño en pantalla de cada tipo de mueble.</summary>
        [Serializable]
        public class SpriteMueble
        {
            public TipoMueble tipo;
            public Sprite sprite;
            [Tooltip("Tamaño en pantalla (los muebles de 2 baldosas, ~2 baldosas de ancho).")]
            public Vector2 tamano = new Vector2(150f, 150f);
        }

        [Header("Tablero")]
        [Tooltip("Contenedor a pantalla completa donde se crean baldosas y personajes.")]
        [SerializeField] private RectTransform _tablero;
        [Tooltip("Separación entre baldosas. Más ancho que alto = suelo visto en ángulo.")]
        [SerializeField] private Vector2 _pasoBaldosa = new Vector2(152f, 104f);
        [SerializeField] private Vector2 _centroTablero = new Vector2(0f, -192f);

        [Header("Cámara (salas más anchas que la pantalla)")]
        [Tooltip("Contenedor del fondo y el tablero: se desplaza de lado para seguir al protagonista.")]
        [SerializeField] private RectTransform _mundo;
        [Tooltip("Ancho total del fondo de la sala, en unidades del Canvas (1920 = una pantalla).")]
        [SerializeField] private float _anchoMundo = 1920f;
        [Tooltip("Qué tan rápido la cámara alcanza al protagonista (más = más pegada).")]
        [SerializeField] private float _rapidezCamara = 5f;

        [Header("Sprites")]
        [SerializeField] private Sprite _spriteBaldosa;
        [SerializeField] private Sprite _spriteSombra;
        [Tooltip("Se usa si faltan los cuadros de caminata.")]
        [SerializeField] private Sprite _spriteProtagonista;
        [Header("Caminata del protagonista (4 cuadros por dirección; el 0 es estar quieto)")]
        [Tooltip("De frente: hacia el jugador.")]
        [SerializeField] private Sprite[] _caminarSur;
        [Tooltip("De espaldas: hacia el fondo de la sala.")]
        [SerializeField] private Sprite[] _caminarNorte;
        [SerializeField] private Sprite[] _caminarEste;
        [SerializeField] private Sprite[] _caminarOeste;
        [Header("Resto de sprites")]
        [SerializeField] private Sprite[] _spritesInvitados;
        [SerializeField] private Sprite _spriteLlave;
        [SerializeField] private Sprite _spritePuertaCerrada;
        [SerializeField] private Sprite _spritePuertaAbierta;
        [SerializeField] private Sprite _spriteGlobo;

        [Header("Muebles (bloquean baldosas, no hablan)")]
        [SerializeField] private SpriteMueble[] _spritesMuebles;

        [Header("Entrada (reloj de ébano). Sin cuadros, se usa la puerta")]
        [Tooltip("Cuadros del reloj cerrado: el péndulo oscila (tic-tac).")]
        [SerializeField] private Sprite[] _cuadrosEntrada;
        [SerializeField] private Sprite _spriteEntradaAbierta;
        [SerializeField] private Vector2 _tamanoEntrada = new Vector2(147f, 204f);
        [SerializeField] private float _tiempoCuadroEntrada = 0.45f;
        [Tooltip("Campanadas antes de que se abra el reloj.")]
        [SerializeField] private int _campanadas = 3;

        [Header("Colores")]
        [SerializeField] private Color _colorBaldosaA = new Color(0.22f, 0.40f, 0.78f, 1f);
        [SerializeField] private Color _colorBaldosaB = new Color(0.17f, 0.33f, 0.68f, 1f);
        [SerializeField] private Color _colorBaldosaPisada = new Color(0.50f, 0.22f, 0.42f, 1f);
        [SerializeField] private Color _colorTrazo = new Color(0.86f, 0.09f, 0.13f, 1f);
        [SerializeField] private Color _colorLlaveFalta = new Color(1f, 1f, 1f, 0.22f);
        [SerializeField] private Color _colorLlaveTiene = Color.white;

        [Header("HUD")]
        [SerializeField] private TMP_Text _textoNombre;
        [SerializeField] private Image[] _iconosLlaves;
        [SerializeField] private TMP_Text _textoAviso;
        [SerializeField] private string _textoControles = "Flechas o WASD: moverse (pisa tu trazo para devolverte)   ·   R: reiniciar   ·   Esc: volver a la selección de capítulos";

        [Header("Narradora (velo negro con texto, al entrar y al salir)")]
        [SerializeField] private CanvasGroup _velo;
        [SerializeField] private TMP_Text _textoNarradora;
        [SerializeField] private ClicView _clicVelo;
        [SerializeField] private float _tiempoVelo = 0.6f;
        [Tooltip("Pausa para ver abrirse la puerta antes de que aparezca el texto de salida.")]
        [SerializeField] private float _pausaAlCompletar = 0.9f;

        [Header("Cinemática al entrar (opcional)")]
        [Tooltip("Video que se ve antes de los textos y del puzle. Espacio avanza, Escape la salta.")]
        [SerializeField] private CinematicaView _cinematica;

        [Header("Entrada del encapuchado")]
        [Tooltip("Lo que tarda en cruzar desde la puerta de entrada hasta su primera baldosa.")]
        [SerializeField] private float _tiempoEntrada = 1.1f;

        [Header("Final de la demo (opcional: sin él, al resolver se vuelve al reloj)")]
        [Tooltip("Contenedor del fondo y el tablero: se desliza a la derecha al cruzar la puerta.")]
        [SerializeField] private RectTransform _escenario;
        [Tooltip("Habitación negra que entra desde la izquierda (hija de Escenario).")]
        [SerializeField] private RectTransform _salaNegra;
        [Tooltip("Nombre de la sala, llaves, aviso y cronómetro: se desvanecen al salir.")]
        [SerializeField] private CanvasGroup _hud;
        [Tooltip("\"Gracias por jugar nuestra demo\" y los créditos.")]
        [SerializeField] private CanvasGroup _finalDemo;
        [SerializeField] private ClicView _clicFinal;
        [SerializeField] private float _tiempoPaneo = 1.8f;
        [Tooltip("Si nadie toca nada, a los cuántos segundos se vuelve solo al menú.")]
        [SerializeField] private float _esperaMaximaFinal = 15f;
        [SerializeField] private string _escenaFinal = "MainMenu";

        [Header("Movimiento")]
        [SerializeField] private float _tiempoPaso = 0.2f;
        [SerializeField] private float _alturaSalto = 16f;

        [Header("Escenas")]
        [SerializeField] private string _escenaReloj = "ChapterSelect";

        [Header("Eventos (sonido)")]
        [SerializeField] private UnityEvent _alDarPaso;
        [SerializeField] private UnityEvent _alRecogerLlave;
        [SerializeField] private UnityEvent _alChocar;
        [SerializeField] private UnityEvent _alCompletar;
        [SerializeField] private UnityEvent _alCerrarPuerta;
        [SerializeField] private UnityEvent _alAbrirPuerta;
        [Tooltip("Cada campanada del reloj de ébano antes de que salga el encapuchado.")]
        [SerializeField] private UnityEvent _alSonarReloj;

        /// <summary>true cuando el jugador puede mover al encapuchado (lo usa el cronómetro).</summary>
        public event Action<bool> AlCambiarJuegoActivo;

        private RoomPuzzleViewModel _viewModel;
        private Action<string> _cargarEscena;
        private DefinicionSala _sala;
        private bool _saliendo;
        private bool _bloqueado = true;       // hasta que termina la entrada
        private bool _animacionManual;        // una corrutina mueve al protagonista
        private bool _enFinal;
        private bool _clicFinalPendiente;

        // Objetos del tablero
        private RectTransform _capaBaldosas, _capaTrazo, _capaSombras, _capaEntidades, _capaGlobos;
        private readonly Dictionary<Celda, Image> _baldosas = new Dictionary<Celda, Image>();
        private readonly Dictionary<Celda, RectTransform> _llaves = new Dictionary<Celda, RectTransform>();
        private readonly Dictionary<Celda, RectTransform> _sombrasLlaves = new Dictionary<Celda, RectTransform>();
        private readonly List<RectTransform> _invitados = new List<RectTransform>();
        private readonly List<CanvasGroup> _globos = new List<CanvasGroup>();
        private readonly List<TMP_Text> _textosGlobos = new List<TMP_Text>();
        private readonly List<float> _alphaGlobos = new List<float>();
        private readonly List<Image> _piezasTrazo = new List<Image>();
        private RectTransform _protagonista, _sombraProtagonista;
        private Image _imagenProtagonista;
        private Direccion _mirando = Direccion.Oeste;      // entra por el este, de cara a la sala
        private int _pasosDados;
        private Image _puerta, _puertaEntrada;
        private Image _imagenSombraProtagonista;
        private bool _entradaAbierta;
        private float _relojTiempo;
        private bool _camaraLista;

        // Animación del protagonista
        private Vector2 _desde, _hacia;
        private float _avancePaso = 1f;
        private Celda _celdaVisual;

        // Velo de la narradora
        private float _alphaVeloObjetivo = 1f;
        private float _esperaVelo;

        /// <param name="cargarEscena">Inyectable para tests; por defecto SceneManager.LoadScene.</param>
        public void Construir(RoomPuzzleViewModel viewModel, Action<string> cargarEscena = null)
        {
            _viewModel = viewModel;
            _cargarEscena = cargarEscena ?? (escena => SceneManager.LoadScene(escena));

            if (_clicVelo != null) _clicVelo.AlClic += Continuar;
            if (_clicFinal != null) _clicFinal.AlClic += AlClicFinal;
            if (_finalDemo != null)
            {
                _finalDemo.alpha = 0f;
                _finalDemo.blocksRaycasts = false;
            }

            _viewModel.OnStateChanged += AplicarEstado;
            _viewModel.OnPaso += AlDarPaso;
            _viewModel.OnSalir += AlSalirDelViewModel;
            _viewModel.Inicializar();

            // El velo negro de la narradora solo arranca visible si la sala tiene textos de entrada;
            // si no, la sala se ve apenas termina (o se salta) la cinemática.
            if (_velo != null)
            {
                bool conTextos = !string.IsNullOrEmpty(_viewModel.EstadoActual.TextoNarradora);
                _velo.alpha = conTextos ? 1f : 0f;
                _velo.blocksRaycasts = conTextos;
            }

            // La sala ya está armada debajo; la cinemática la tapa hasta que termina o se salta, y
            // recién ahí entra el encapuchado.
            if (_cinematica != null) _cinematica.Reproducir(IniciarEntrada);
            else IniciarEntrada();
        }

        private void IniciarEntrada()
        {
            if (_sala == null || _protagonista == null) { Desbloquear(); return; }
            StartCoroutine(Entrada());
        }

        private void Desbloquear()
        {
            _animacionManual = false;
            _bloqueado = false;
            AlCambiarJuegoActivo?.Invoke(true);
        }

        private void OnDestroy()
        {
            if (_clicVelo != null) _clicVelo.AlClic -= Continuar;
            if (_clicFinal != null) _clicFinal.AlClic -= AlClicFinal;
            if (_viewModel == null) return;
            _viewModel.OnStateChanged -= AplicarEstado;
            _viewModel.OnPaso -= AlDarPaso;
            _viewModel.OnSalir -= AlSalirDelViewModel;
        }

        // ------------------------------------------------------------------ input

        private void Update()
        {
            if (_viewModel == null || _sala == null) return;

            AnimarProtagonista();
            AnimarCamara();
            AnimarEntrada();
            AnimarAmbiente();
            AnimarVelo();
            if (_saliendo) return;

            // Mientras corre la cinemática el input es suyo (y la tecla que la cierra no cuenta aquí).
            if (_cinematica != null && (_cinematica.Reproduciendo || _cinematica.FrameFin == Time.frameCount)) return;

            // Durante la entrada y el final manda la animación.
            if (_bloqueado) return;

            // Escape vuelve al reloj en cualquier momento (si la sala ya se resolvió, ya quedó guardada).
            if (MenuInput.Cancelar) { Salir(); return; }

            var fase = _viewModel.EstadoActual.Fase;
            if (fase != FaseSala.Jugando)
            {
                if (MenuInput.Confirmar) Continuar();
                return;
            }

            if (MenuInput.Reiniciar) { _viewModel.Reiniciar(); return; }
            if (MenuInput.Deshacer) { _viewModel.Deshacer(); return; }

            if (_avancePaso < 1f) return;     // espera a que termine el salto anterior
            if (MenuInput.Arriba) _viewModel.Mover(Direccion.Norte);
            else if (MenuInput.Abajo) _viewModel.Mover(Direccion.Sur);
            else if (MenuInput.Izquierda) _viewModel.Mover(Direccion.Oeste);
            else if (MenuInput.Derecha) _viewModel.Mover(Direccion.Este);
        }

        private void Continuar()
        {
            // Mientras el velo todavía está apareciendo no se pasa de texto (evita saltárselo sin leer).
            if (_viewModel == null || _saliendo || _esperaVelo > 0f) return;
            if (_velo != null && _velo.alpha < 0.9f) return;
            _viewModel.Continuar();
        }

        private void AlClicBaldosa(Celda celda)
        {
            if (_viewModel != null && !_saliendo && !_bloqueado && _avancePaso >= 1f) _viewModel.MoverA(celda);
        }

        /// <summary>Si hay final de la demo, la salida la decide su animación, no el ViewModel.</summary>
        private void AlSalirDelViewModel()
        {
            if (_enFinal) return;
            Salir();
        }

        private void AlClicFinal() => _clicFinalPendiente = true;

        private void Salir()
        {
            if (_saliendo) return;
            _saliendo = true;
            _cargarEscena(_escenaReloj);
        }

        // ------------------------------------------------------------------ reacción al ViewModel

        private void AlDarPaso(ResultadoPaso resultado)
        {
            switch (resultado)
            {
                case ResultadoPaso.Bloqueado: _alChocar?.Invoke(); break;
                case ResultadoPaso.LlaveRecogida: _alRecogerLlave?.Invoke(); break;
                case ResultadoPaso.Completada:
                    _alCompletar?.Invoke();
                    if (_finalDemo != null)
                    {
                        _enFinal = true;
                        _bloqueado = true;
                        AlCambiarJuegoActivo?.Invoke(false);
                        StartCoroutine(Final());
                    }
                    else _esperaVelo = _pausaAlCompletar;
                    break;
                default: _alDarPaso?.Invoke(); break;
            }
        }

        private void AplicarEstado(RoomPuzzleViewState estado)
        {
            if (_sala == null)
            {
                _sala = estado.Sala;
                CrearTablero();
            }

            // Protagonista: salta hacia su nueva baldosa.
            if (estado.Posicion != _celdaVisual)
            {
                int dx = estado.Posicion.X - _celdaVisual.X;
                int dy = estado.Posicion.Y - _celdaVisual.Y;
                if (Math.Abs(dx) >= Math.Abs(dy)) _mirando = dx > 0 ? Direccion.Este : Direccion.Oeste;
                else _mirando = dy > 0 ? Direccion.Norte : Direccion.Sur;
                _pasosDados++;
                _desde = _sombraProtagonista.anchoredPosition + new Vector2(0f, 26f);
                _hacia = PosicionDe(estado.Posicion);
                _avancePaso = 0f;
                _celdaVisual = estado.Posicion;
            }

            // Baldosas pisadas y trazo.
            var pisadas = new HashSet<Celda>(estado.Camino);
            foreach (var par in _baldosas)
                par.Value.color = pisadas.Contains(par.Key) ? _colorBaldosaPisada : ColorBase(par.Key);
            PintarTrazo(estado.Camino);

            // Llaves: desaparecen del suelo al recogerlas y se encienden en el HUD.
            foreach (var par in _llaves)
            {
                bool recogida = false;
                foreach (var c in estado.LlavesRecogidas)
                    if (c == par.Key) { recogida = true; break; }
                par.Value.gameObject.SetActive(!recogida);
                if (_sombrasLlaves.TryGetValue(par.Key, out var sombra)) sombra.gameObject.SetActive(!recogida);
            }
            if (_iconosLlaves != null)
                for (int i = 0; i < _iconosLlaves.Length; i++)
                    if (_iconosLlaves[i] != null)
                    {
                        _iconosLlaves[i].gameObject.SetActive(i < estado.TotalLlaves);
                        _iconosLlaves[i].color = i < estado.Llaves ? _colorLlaveTiene : _colorLlaveFalta;
                    }

            // La puerta de salida solo se abre al resolver la sala (con final de la demo la abre su animación).
            if (_puerta != null && !_enFinal)
            {
                bool abrir = estado.Fase == FaseSala.Salida && _finalDemo == null && _spritePuertaAbierta != null;
                _puerta.sprite = abrir ? _spritePuertaAbierta : _spritePuertaCerrada;
            }

            // Globos de los invitados que tienen al protagonista al lado.
            for (int i = 0; i < _globos.Count && i < estado.FrasesDeInvitados.Count; i++)
            {
                string frase = estado.FrasesDeInvitados[i];
                _alphaGlobos[i] = string.IsNullOrEmpty(frase) ? 0f : 1f;
                if (!string.IsNullOrEmpty(frase)) _textosGlobos[i].text = frase;
            }

            if (_textoAviso != null)
            {
                if (estado.PuertaNoCede) _textoAviso.text = "La puerta no cede: faltan llaves.   Devuélvete por tu trazo o pulsa R para reiniciar.";
                else if (estado.Atascada) _textoAviso.text = "No queda por dónde seguir.   Devuélvete por tu trazo o pulsa R para reiniciar.";
                else _textoAviso.text = _textoControles;
            }

            // Narradora.
            bool conTexto = !string.IsNullOrEmpty(estado.TextoNarradora);
            _alphaVeloObjetivo = conTexto ? 1f : 0f;
            if (conTexto && _textoNarradora != null) _textoNarradora.text = estado.TextoNarradora;

            OrdenarEntidades();
        }

        // ------------------------------------------------------------------ construcción del tablero

        private void CrearTablero()
        {
            _capaBaldosas = CrearCapa("Baldosas");
            _capaTrazo = CrearCapa("Trazo");
            _capaSombras = CrearCapa("Sombras");
            _capaEntidades = CrearCapa("Entidades");
            _capaGlobos = CrearCapa("Globos");

            if (_textoNombre != null) _textoNombre.text = _sala.Nombre;

            // Baldosas: de la fila del fondo hacia la de delante, para que el canto de cada una
            // quede tapado por la siguiente.
            for (int y = _sala.Alto - 1; y >= 0; y--)
            {
                for (int x = 0; x < _sala.Ancho; x++)
                {
                    var celda = new Celda(x, y);
                    var img = CrearImagen($"Baldosa_{x}_{y}", _capaBaldosas, _spriteBaldosa, ColorBase(celda),
                        PosicionDe(celda), new Vector2(_pasoBaldosa.x, _pasoBaldosa.y), true);
                    img.gameObject.AddComponent<BaldosaView>().Construir(celda, AlClicBaldosa);
                    _baldosas[celda] = img;
                }
            }

            // Puerta de salida (oeste) y de entrada (este), fuera de la grilla. Sus pies quedan un
            // poco más al fondo que los del protagonista, para que él pase por delante.
            var tamanoPuerta = new Vector2(147f, 204f);
            _puerta = CrearDePie("Puerta", _spritePuertaCerrada, Color.white,
                PuntoPuerta(new Celda(-1, _sala.Salida.Y), 20f) + new Vector2(0f, -28f), tamanoPuerta);
            // La entrada es el reloj de ébano (o, si no hay cuadros, una puerta que arranca abierta).
            bool conReloj = HayReloj;
            _entradaAbierta = !conReloj;
            Sprite spriteEntrada = conReloj ? _cuadrosEntrada[0]
                : _spritePuertaAbierta != null ? _spritePuertaAbierta : _spritePuertaCerrada;
            _puertaEntrada = CrearDePie("Entrada", spriteEntrada, Color.white,
                PuntoPuerta(new Celda(_sala.Ancho, _sala.Entrada.Y), -20f) + new Vector2(0f, -28f),
                conReloj ? _tamanoEntrada : tamanoPuerta);

            // Muebles.
            foreach (var mueble in _sala.Muebles) CrearMueble(mueble);

            // Llaves.
            foreach (var celda in _sala.Llaves)
            {
                _sombrasLlaves[celda] = CrearSombra(PosicionDe(celda), 0.7f);
                var llave = CrearDePie("Llave", _spriteLlave, Color.white, PosicionDe(celda) + new Vector2(0f, -8f), new Vector2(63f, 63f));
                _llaves[celda] = llave.rectTransform;
            }

            // Invitados, cada uno con su globo.
            // Personajes a 3 unidades por píxel (38 x 48 px): caben en su baldosa sin tapar la de atrás.
            var tamanoPersonaje = new Vector2(114f, 144f);
            for (int i = 0; i < _sala.Invitados.Count; i++)
            {
                var invitado = _sala.Invitados[i];
                var pos = PosicionDe(invitado.Celda);
                CrearSombra(pos, 1f);
                Sprite sprite = null;
                if (_spritesInvitados != null && _spritesInvitados.Length > 0)
                    sprite = _spritesInvitados[Math.Abs(invitado.Aspecto) % _spritesInvitados.Length];
                var img = CrearDePie($"Invitado_{i + 1}", sprite, Color.white, pos + new Vector2(0f, -30f), tamanoPersonaje);
                _invitados.Add(img.rectTransform);
                CrearGlobo(pos + new Vector2(0f, 120f));
            }

            // Protagonista.
            // Protagonista: arranca en el umbral de la puerta de entrada (invisible hasta que entra).
            _celdaVisual = _sala.Entrada;
            var inicio = PosicionDe(_sala.Entrada);
            _sombraProtagonista = CrearSombra(inicio, 1f);
            _imagenSombraProtagonista = _sombraProtagonista.GetComponent<Image>();
            _imagenProtagonista = CrearDePie("Protagonista", _spriteProtagonista, Color.white, inicio, tamanoPersonaje);
            _protagonista = _imagenProtagonista.rectTransform;
            _desde = _hacia = inicio;
            ColocarProtagonista(PuntoPuerta(new Celda(_sala.Ancho, _sala.Entrada.Y), -20f), 0f);
            FijarOpacidadProtagonista(0f);
            AnimarCamara();      // la primera vez se coloca de golpe
        }

        private bool HayReloj => _cuadrosEntrada != null && _cuadrosEntrada.Length > 0 && _cuadrosEntrada[0] != null;

        private void CrearMueble(MuebleSala mueble)
        {
            SpriteMueble datos = null;
            if (_spritesMuebles != null)
                foreach (var d in _spritesMuebles)
                    if (d != null && d.tipo == mueble.Tipo) { datos = d; break; }

            // Centro de las baldosas que ocupa; los pies, un poco más abajo que los de los personajes.
            var izquierda = PosicionDe(mueble.Celda);
            var derecha = PosicionDe(new Celda(mueble.Celda.X + mueble.Ancho - 1, mueble.Celda.Y));
            var pies = (izquierda + derecha) * 0.5f + new Vector2(0f, -36f);
            var tamano = datos != null ? datos.tamano : new Vector2(_pasoBaldosa.x * mueble.Ancho * 0.9f, 120f);
            var img = CrearDePie($"{mueble.Tipo}_{mueble.Celda.X}_{mueble.Celda.Y}",
                datos != null ? datos.sprite : null,
                datos != null && datos.sprite != null ? Color.white : new Color(0.25f, 0.12f, 0.12f, 1f), pies, tamano);
            img.preserveAspect = datos != null && datos.sprite != null;
        }

        private RectTransform CrearCapa(string nombre)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(_tablero, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            return rt;
        }

        private static Image CrearImagen(string nombre, Transform padre, Sprite sprite, Color color,
            Vector2 posicion, Vector2 tamano, bool recibeClic = false)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(padre, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = posicion;
            rt.sizeDelta = tamano;
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = recibeClic;
            return img;
        }

        /// <summary>Imagen "de pie" sobre el suelo: su pivot está en los pies, para ordenar por profundidad.</summary>
        private Image CrearDePie(string nombre, Sprite sprite, Color color, Vector2 pies, Vector2 tamano)
        {
            var img = CrearImagen(nombre, _capaEntidades, sprite, color, pies, tamano);
            img.rectTransform.pivot = new Vector2(0.5f, 0f);
            img.rectTransform.anchoredPosition = pies;
            return img;
        }

        private RectTransform CrearSombra(Vector2 centro, float escala)
        {
            var img = CrearImagen("Sombra", _capaSombras, _spriteSombra, Color.white,
                centro + new Vector2(0f, -26f), new Vector2(96f, 36f) * escala);
            return img.rectTransform;
        }

        private void CrearGlobo(Vector2 base_)
        {
            var img = CrearImagen("Globo", _capaGlobos, _spriteGlobo, Color.white, base_, new Vector2(255f, 102f));
            img.rectTransform.pivot = new Vector2(0.5f, 0f);
            img.rectTransform.anchoredPosition = base_;
            var grupo = img.gameObject.AddComponent<CanvasGroup>();
            grupo.alpha = 0f;
            grupo.blocksRaycasts = false;
            grupo.interactable = false;

            var textoGo = new GameObject("Texto", typeof(RectTransform));
            var rt = (RectTransform)textoGo.transform;
            rt.SetParent(img.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(14f, 28f);      // deja libre la punta del globo
            rt.offsetMax = new Vector2(-14f, -8f);
            var texto = textoGo.AddComponent<TextMeshProUGUI>();
            texto.fontSize = 20f;
            texto.color = new Color(0.08f, 0.06f, 0.16f, 1f);
            texto.alignment = TextAlignmentOptions.Center;
            texto.raycastTarget = false;

            _globos.Add(grupo);
            _textosGlobos.Add(texto);
            _alphaGlobos.Add(0f);
        }

        private Vector2 PosicionDe(Celda celda)
        {
            float x = (celda.X - (_sala.Ancho - 1) * 0.5f) * _pasoBaldosa.x;
            float y = (celda.Y - (_sala.Alto - 1) * 0.5f) * _pasoBaldosa.y;
            return _centroTablero + new Vector2(x, y);
        }

        /// <summary>Punto del suelo frente a una puerta (las puertas están en celdas fuera de la grilla).</summary>
        private Vector2 PuntoPuerta(Celda celdaPuerta, float desplazamientoX) =>
            PosicionDe(celdaPuerta) + new Vector2(desplazamientoX, 0f);

        private Color ColorBase(Celda celda) => (celda.X + celda.Y) % 2 == 0 ? _colorBaldosaA : _colorBaldosaB;

        // ------------------------------------------------------------------ trazo

        /// <summary>El trazo rojo: un nudo en cada baldosa pisada y un tramo recto entre baldosas seguidas.</summary>
        private void PintarTrazo(IReadOnlyList<Celda> camino)
        {
            int usadas = 0;
            for (int i = 0; i < camino.Count; i++)
            {
                var centro = PosicionDe(camino[i]);
                Pieza(usadas++, centro, new Vector2(28f, 20f));
                if (i == 0) continue;

                var anterior = PosicionDe(camino[i - 1]);
                var medio = (centro + anterior) * 0.5f;
                bool horizontal = Mathf.Abs(centro.x - anterior.x) > Mathf.Abs(centro.y - anterior.y);
                var tamano = horizontal
                    ? new Vector2(Mathf.Abs(centro.x - anterior.x), 12f)
                    : new Vector2(16f, Mathf.Abs(centro.y - anterior.y));
                Pieza(usadas++, medio, tamano);
            }
            for (int i = usadas; i < _piezasTrazo.Count; i++) _piezasTrazo[i].gameObject.SetActive(false);
        }

        private void Pieza(int indice, Vector2 posicion, Vector2 tamano)
        {
            if (indice >= _piezasTrazo.Count)
                _piezasTrazo.Add(CrearImagen("Trazo", _capaTrazo, null, _colorTrazo, Vector2.zero, Vector2.zero));
            var img = _piezasTrazo[indice];
            img.gameObject.SetActive(true);
            img.color = _colorTrazo;
            img.rectTransform.anchoredPosition = posicion;
            img.rectTransform.sizeDelta = tamano;
        }

        // ------------------------------------------------------------------ animación

        private void AnimarProtagonista()
        {
            if (_protagonista == null || _animacionManual) return;
            bool caminando = _avancePaso < 1f;
            if (caminando)
            {
                _avancePaso = Mathf.Min(1f, _avancePaso + Time.unscaledDeltaTime / Mathf.Max(0.01f, _tiempoPaso));
                float e = Mathf.SmoothStep(0f, 1f, _avancePaso);
                ColocarProtagonista(Vector2.Lerp(_desde, _hacia, e), Mathf.Sin(_avancePaso * Mathf.PI) * _alturaSalto);
                if (_avancePaso >= 1f) OrdenarEntidades();
            }

            // Cuadro de la caminata: dos por paso, alternando el pie; quieto = cuadro 0.
            var cuadros = CuadrosDe(_mirando);
            if (cuadros != null && cuadros.Length > 0 && _imagenProtagonista != null)
            {
                int indice = caminando ? (_pasosDados * 2 + (_avancePaso < 0.5f ? 1 : 2)) % cuadros.Length : 0;
                if (cuadros[indice] != null) _imagenProtagonista.sprite = cuadros[indice];
            }
        }

        private Sprite[] CuadrosDe(Direccion direccion)
        {
            switch (direccion)
            {
                case Direccion.Norte: return _caminarNorte;
                case Direccion.Sur: return _caminarSur;
                case Direccion.Este: return _caminarEste;
                default: return _caminarOeste;
            }
        }

        private void ColocarProtagonista(Vector2 suelo, float altura)
        {
            _protagonista.anchoredPosition = suelo + new Vector2(0f, -30f + altura);
            if (_sombraProtagonista != null) _sombraProtagonista.anchoredPosition = suelo + new Vector2(0f, -26f);
        }

        /// <summary>La cámara sigue al protagonista de lado, sin mostrar más allá del fondo de la sala.</summary>
        private void AnimarCamara()
        {
            if (_mundo == null || _sombraProtagonista == null) return;
            float pantalla = _escenario != null && _escenario.rect.width > 1f ? _escenario.rect.width : 1920f;
            float limite = Mathf.Max(0f, (_anchoMundo - pantalla) * 0.5f);
            float objetivo = Mathf.Clamp(-_sombraProtagonista.anchoredPosition.x, -limite, limite);
            var pos = _mundo.anchoredPosition;
            pos.x = _camaraLista
                ? Mathf.Lerp(pos.x, objetivo, 1f - Mathf.Exp(-_rapidezCamara * Time.unscaledDeltaTime))
                : objetivo;
            _mundo.anchoredPosition = pos;
            _camaraLista = true;
        }

        /// <summary>El péndulo del reloj de ébano oscila mientras está cerrado.</summary>
        private void AnimarEntrada()
        {
            if (_puertaEntrada == null || _entradaAbierta || !HayReloj) return;
            _relojTiempo += Time.unscaledDeltaTime;
            int i = (int)(_relojTiempo / Mathf.Max(0.05f, _tiempoCuadroEntrada)) % _cuadrosEntrada.Length;
            if (_cuadrosEntrada[i] != null) _puertaEntrada.sprite = _cuadrosEntrada[i];
        }

        private void AnimarAmbiente()
        {
            float t = Time.unscaledTime;
            float dt = Time.unscaledDeltaTime;

            // Los invitados festejan: dan saltitos, cada uno a su ritmo. Los desplazamientos van de a
            // 3 unidades (un píxel del arte) para que el pixel art no se vea borroso.
            for (int i = 0; i < _invitados.Count; i++)
            {
                var pies = PosicionDe(_sala.Invitados[i].Celda) + new Vector2(0f, -30f);
                float salto = Mathf.Sin(t * (5.5f + i * 0.9f) + i * 1.7f) > 0.2f ? 3f : 0f;
                _invitados[i].anchoredPosition = pies + new Vector2(0f, salto);
            }

            // Las llaves flotan.
            int n = 0;
            foreach (var par in _llaves)
            {
                var basePos = PosicionDe(par.Key) + new Vector2(0f, -8f);
                par.Value.anchoredPosition = basePos + new Vector2(0f, Mathf.Round(Mathf.Sin(t * 2.6f + n * 1.3f) * 1.5f) * 3f);
                n++;
            }

            // Los globos aparecen y desaparecen con suavidad.
            for (int i = 0; i < _globos.Count; i++)
                _globos[i].alpha = Mathf.MoveTowards(_globos[i].alpha, _alphaGlobos[i], dt * 6f);
        }

        private void AnimarVelo()
        {
            if (_velo == null) return;
            if (_esperaVelo > 0f)
            {
                _esperaVelo -= Time.unscaledDeltaTime;
                return;
            }
            float paso = Time.unscaledDeltaTime / Mathf.Max(0.01f, _tiempoVelo);
            _velo.alpha = Mathf.MoveTowards(_velo.alpha, _alphaVeloObjetivo, paso);
            _velo.blocksRaycasts = _velo.alpha > 0.05f;
        }

        // ------------------------------------------------------------------ entrada y final

        private void FijarOpacidadProtagonista(float alpha)
        {
            if (_imagenProtagonista != null)
            {
                var c = _imagenProtagonista.color; c.a = alpha; _imagenProtagonista.color = c;
            }
            if (_imagenSombraProtagonista != null)
            {
                var c = _imagenSombraProtagonista.color; c.a = alpha; _imagenSombraProtagonista.color = c;
            }
        }

        /// <summary>Camina en línea recta mirando hacia 'direccion', con el ciclo de pasos.</summary>
        private IEnumerator Caminar(Vector2 desde, Vector2 hacia, float duracion, Direccion direccion,
            float alphaInicio, float alphaFin)
        {
            _animacionManual = true;
            _mirando = direccion;
            var cuadros = CuadrosDe(direccion);
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.unscaledDeltaTime / Mathf.Max(0.01f, duracion));
                ColocarProtagonista(Vector2.Lerp(desde, hacia, t), 0f);
                FijarOpacidadProtagonista(Mathf.Lerp(alphaInicio, alphaFin, Mathf.Clamp01(t * 3f)));
                if (cuadros != null && cuadros.Length > 0 && _imagenProtagonista != null)
                {
                    int indice = (int)(t * duracion / 0.14f) % cuadros.Length;
                    if (cuadros[indice] != null) _imagenProtagonista.sprite = cuadros[indice];
                }
                yield return null;
            }
            if (cuadros != null && cuadros.Length > 0 && cuadros[0] != null) _imagenProtagonista.sprite = cuadros[0];
        }

        /// <summary>El encapuchado sale de la puerta de entrada hasta su baldosa y la puerta se cierra tras él.</summary>
        private IEnumerator Entrada()
        {
            _bloqueado = true;
            var umbral = PuntoPuerta(new Celda(_sala.Ancho, _sala.Entrada.Y), -20f);
            yield return new WaitForSecondsRealtime(0.25f);

            // El reloj da la medianoche y se abre: de él sale la máscara.
            if (HayReloj && _puertaEntrada != null)
            {
                for (int i = 0; i < _campanadas; i++)
                {
                    _alSonarReloj?.Invoke();
                    yield return Sacudir(_puertaEntrada.rectTransform, 0.18f, 3f);
                    yield return new WaitForSecondsRealtime(0.32f);
                }
                _entradaAbierta = true;
                if (_spriteEntradaAbierta != null) _puertaEntrada.sprite = _spriteEntradaAbierta;
                _alAbrirPuerta?.Invoke();
                yield return new WaitForSecondsRealtime(0.3f);
            }

            yield return Caminar(umbral, PosicionDe(_sala.Entrada), _tiempoEntrada, Direccion.Oeste, 0f, 1f);
            FijarOpacidadProtagonista(1f);
            OrdenarEntidades();

            yield return new WaitForSecondsRealtime(0.2f);
            if (_puertaEntrada != null)
            {
                // Se cierra tras él (el reloj vuelve a su tic-tac).
                if (HayReloj) { _entradaAbierta = false; _puertaEntrada.sprite = _cuadrosEntrada[0]; }
                else _puertaEntrada.sprite = _spritePuertaCerrada;
                _alCerrarPuerta?.Invoke();
                yield return Sacudir(_puertaEntrada.rectTransform, 0.25f, 3f);
            }
            yield return new WaitForSecondsRealtime(0.15f);
            Desbloquear();
        }

        /// <summary>Al resolver: se abre la puerta, el encapuchado la cruza y la cámara pasa a la sala negra.</summary>
        private IEnumerator Final()
        {
            // Termina el salto a la baldosa de salida.
            while (_avancePaso < 1f) yield return null;
            for (int i = 0; i < _alphaGlobos.Count; i++) _alphaGlobos[i] = 0f;
            yield return new WaitForSecondsRealtime(0.35f);

            // Se abre la puerta.
            if (_puerta != null)
            {
                if (_spritePuertaAbierta != null) _puerta.sprite = _spritePuertaAbierta;
                _alAbrirPuerta?.Invoke();
                yield return Latir(_puerta.rectTransform, 0.25f, 0.08f);
            }
            yield return new WaitForSecondsRealtime(0.35f);

            // La cruza (se pierde en la oscuridad del vano) mientras se apaga el HUD.
            var umbral = PuntoPuerta(new Celda(-1, _sala.Salida.Y), 20f);
            StartCoroutine(Desvanecer(_hud, 0.6f));
            yield return Caminar(PosicionDe(_sala.Salida), umbral, 0.8f, Direccion.Oeste, 1f, 1f);
            float f = 0f;
            while (f < 1f)
            {
                f = Mathf.Min(1f, f + Time.unscaledDeltaTime / 0.3f);
                FijarOpacidadProtagonista(1f - f);
                yield return null;
            }
            yield return new WaitForSecondsRealtime(0.2f);

            // El escenario se desliza a la derecha: pasamos a la habitación negra.
            if (_escenario != null)
            {
                float ancho = _escenario.rect.width > 1f ? _escenario.rect.width : 1920f;
                if (_salaNegra != null)
                {
                    _salaNegra.gameObject.SetActive(true);
                    _salaNegra.anchoredPosition = new Vector2(-ancho, 0f);
                }
                var origen = _escenario.anchoredPosition;
                float p = 0f;
                while (p < 1f)
                {
                    p = Mathf.Min(1f, p + Time.unscaledDeltaTime / Mathf.Max(0.01f, _tiempoPaneo));
                    _escenario.anchoredPosition = origen + new Vector2(Mathf.SmoothStep(0f, ancho, p), 0f);
                    yield return null;
                }
            }
            yield return new WaitForSecondsRealtime(0.5f);

            // Gracias por jugar.
            if (_finalDemo != null)
            {
                float a = 0f;
                while (a < 1f)
                {
                    a = Mathf.Min(1f, a + Time.unscaledDeltaTime / 1.2f);
                    _finalDemo.alpha = a;
                    yield return null;
                }
                _finalDemo.blocksRaycasts = true;
            }

            _clicFinalPendiente = false;
            float espera = 0f;
            while (espera < _esperaMaximaFinal)
            {
                espera += Time.unscaledDeltaTime;
                if (espera > 0.6f && (MenuInput.Confirmar || MenuInput.Cancelar || _clicFinalPendiente)) break;
                yield return null;
            }

            if (_finalDemo != null)
            {
                float a = 1f;
                while (a > 0f)
                {
                    a = Mathf.Max(0f, a - Time.unscaledDeltaTime / 0.6f);
                    _finalDemo.alpha = a;
                    yield return null;
                }
            }
            _saliendo = true;
            _cargarEscena(_escenaFinal);
        }

        private static IEnumerator Desvanecer(CanvasGroup grupo, float duracion)
        {
            if (grupo == null) yield break;
            float inicio = grupo.alpha, t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.unscaledDeltaTime / Mathf.Max(0.01f, duracion));
                grupo.alpha = Mathf.Lerp(inicio, 0f, t);
                yield return null;
            }
        }

        /// <summary>Temblor horizontal de a 'paso' unidades (un píxel del arte), como un portazo.</summary>
        private static IEnumerator Sacudir(RectTransform rt, float duracion, float paso)
        {
            var origen = rt.anchoredPosition;
            float t = 0f;
            int i = 0;
            while (t < duracion)
            {
                t += Time.unscaledDeltaTime;
                rt.anchoredPosition = origen + new Vector2((i++ % 2 == 0 ? 1f : -1f) * paso, 0f);
                yield return null;
            }
            rt.anchoredPosition = origen;
        }

        /// <summary>Crece un poco y vuelve, para marcar que la puerta se abrió.</summary>
        private static IEnumerator Latir(RectTransform rt, float duracion, float extra)
        {
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.unscaledDeltaTime / Mathf.Max(0.01f, duracion));
                float s = 1f + Mathf.Sin(t * Mathf.PI) * extra;
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        /// <summary>Quien está más al fondo (más arriba en pantalla) se dibuja primero.</summary>
        private void OrdenarEntidades()
        {
            if (_capaEntidades == null) return;
            var hijos = new List<RectTransform>();
            foreach (Transform hijo in _capaEntidades) hijos.Add((RectTransform)hijo);
            hijos.Sort((a, b) => b.anchoredPosition.y.CompareTo(a.anchoredPosition.y));
            for (int i = 0; i < hijos.Count; i++) hijos[i].SetSiblingIndex(i);
        }
    }
}
