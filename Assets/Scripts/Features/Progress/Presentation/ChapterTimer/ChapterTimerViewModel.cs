using System;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Presentation.ChapterTimer
{
    /// <summary>
    /// Cronómetro de un capítulo. En pantalla muestra el tiempo de ESTE intento (siempre parte en
    /// 0 al entrar), pero en la partida guardada suma todo lo jugado en el capítulo (lo que se ve
    /// como "tiempo de partida" en las velas). Persiste cada cierto intervalo y al finalizar, para
    /// no escribir el archivo en cada frame.
    /// </summary>
    public class ChapterTimerViewModel
    {
        private readonly ObtenerProgresoCapituloUseCase _obtenerProgreso;
        private readonly RegistrarLlaveUseCase _registrarLlave;
        private readonly RegistrarTiempoJugadoUseCase _registrarTiempo;
        private readonly CompletarCapituloUseCase _completarCapitulo;
        private readonly float _intervaloGuardado;

        private string _idCapitulo;
        private int _llaves;
        private float _segundos;          // total guardado del capítulo
        private float _segundosIntento;   // lo que va de esta entrada al capítulo
        private bool _completado;
        private float _pendiente;         // segundos aún no persistidos
        private float _desdeUltimoGuardado;

        public ChapterTimerViewState EstadoActual { get; private set; }
        public event Action<ChapterTimerViewState> OnStateChanged;

        public ChapterTimerViewModel(
            ObtenerProgresoCapituloUseCase obtenerProgreso,
            RegistrarLlaveUseCase registrarLlave,
            RegistrarTiempoJugadoUseCase registrarTiempo,
            CompletarCapituloUseCase completarCapitulo,
            float intervaloGuardadoSegundos = 10f)
        {
            _obtenerProgreso = obtenerProgreso;
            _registrarLlave = registrarLlave;
            _registrarTiempo = registrarTiempo;
            _completarCapitulo = completarCapitulo;
            _intervaloGuardado = intervaloGuardadoSegundos;
        }

        public void Inicializar(string idCapitulo)
        {
            _idCapitulo = idCapitulo;
            var progreso = _obtenerProgreso.Ejecutar(idCapitulo);
            _llaves = progreso.Llaves;
            _segundos = progreso.SegundosJugados;
            _segundosIntento = 0f;
            _completado = progreso.Completado;
            _pendiente = 0f;
            _desdeUltimoGuardado = 0f;
            Publicar();
        }

        public void AvanzarTiempo(float delta)
        {
            if (_idCapitulo == null || delta <= 0f || float.IsNaN(delta)) return;

            int segundoAntes = (int)_segundosIntento;
            _segundos += delta;
            _segundosIntento += delta;
            _pendiente += delta;
            _desdeUltimoGuardado += delta;

            if (_desdeUltimoGuardado >= _intervaloGuardado) GuardarPendiente();
            if ((int)_segundosIntento != segundoAntes) Publicar();
        }

        public void AgregarLlave(int cantidad = 1)
        {
            if (_idCapitulo == null) return;
            GuardarPendiente();
            _llaves = _registrarLlave.Ejecutar(_idCapitulo, cantidad).Llaves;
            Publicar();
        }

        public void Completar()
        {
            if (_idCapitulo == null) return;
            GuardarPendiente();
            _completarCapitulo.Ejecutar(_idCapitulo);
            _completado = true;
            Publicar();
        }

        /// <summary>Persiste lo pendiente (al salir de la escena o pausar la app).</summary>
        public void Finalizar()
        {
            if (_idCapitulo == null) return;
            GuardarPendiente();
        }

        private void GuardarPendiente()
        {
            if (_pendiente > 0f)
            {
                _registrarTiempo.Ejecutar(_idCapitulo, _pendiente);
                _pendiente = 0f;
            }
            _desdeUltimoGuardado = 0f;
        }

        private void Publicar()
        {
            EstadoActual = new ChapterTimerViewState(
                idCapitulo: _idCapitulo,
                llaves: _llaves,
                segundosJugados: _segundos,
                tiempoFormateado: ReglasProgreso.FormatearTiempo(_segundosIntento),
                completado: _completado,
                segundosIntento: _segundosIntento
            );
            OnStateChanged?.Invoke(EstadoActual);
        }
    }
}
