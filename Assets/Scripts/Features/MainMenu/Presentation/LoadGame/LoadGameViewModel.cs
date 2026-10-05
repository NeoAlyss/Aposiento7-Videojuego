using System;
using System.Collections.Generic;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.MainMenu.Presentation.LoadGame
{
    /// <summary>
    /// Pantalla de partidas guardadas: una vela por ranura (encendida = hay partida). Sirve para
    /// "Cargar partida" y, cuando todas las ranuras están ocupadas, para elegir cuál reemplaza una
    /// "Nueva partida". No conoce Unity.
    /// </summary>
    public class LoadGameViewModel
    {
        private readonly ObtenerRanurasUseCase _obtenerRanuras;
        private readonly CargarRanuraUseCase _cargarRanura;
        private readonly IniciarPartidaEnRanuraUseCase _iniciarPartida;

        private IReadOnlyList<ResumenRanura> _ranuras = new List<ResumenRanura>();
        private ModoPartidas _modo = ModoPartidas.Cargar;
        private int _indice = LoadGameViewState.SIN_SELECCION;
        private bool _visible;
        private bool _entrando;

        public LoadGameViewState EstadoActual { get; private set; }

        public event Action<LoadGameViewState> OnStateChanged;
        /// <summary>Ya hay una ranura activa lista para jugar: toca ir a la selección de capítulo.</summary>
        public event Action OnEntrarAPartida;
        /// <summary>Se intentó cargar una vela apagada (índice de la vela).</summary>
        public event Action<int> OnRanuraVacia;

        public LoadGameViewModel(
            ObtenerRanurasUseCase obtenerRanuras,
            CargarRanuraUseCase cargarRanura,
            IniciarPartidaEnRanuraUseCase iniciarPartida)
        {
            _obtenerRanuras = obtenerRanuras ?? throw new ArgumentNullException(nameof(obtenerRanuras));
            _cargarRanura = cargarRanura ?? throw new ArgumentNullException(nameof(cargarRanura));
            _iniciarPartida = iniciarPartida ?? throw new ArgumentNullException(nameof(iniciarPartida));
        }

        public void Inicializar()
        {
            _ranuras = _obtenerRanuras.Ejecutar();
            _visible = false;
            _entrando = false;
            _indice = LoadGameViewState.SIN_SELECCION;
            Publicar();
        }

        /// <summary>
        /// "Nueva partida": usa la primera ranura libre y entra directo. Si no queda ninguna, abre las
        /// velas para elegir cuál reemplazar.
        /// </summary>
        public void SolicitarNuevaPartida()
        {
            if (_entrando) return;
            _ranuras = _obtenerRanuras.Ejecutar();
            int libre = ReglasRanuras.PrimeraLibre(_ranuras);
            if (libre != ReglasRanuras.SIN_RANURA && _iniciarPartida.Ejecutar(libre))
            {
                Entrar();
                return;
            }
            Abrir(ModoPartidas.Reemplazar);
        }

        /// <summary>"Cargar partida": abre las velas para elegir cuál continuar.</summary>
        public void AbrirCarga()
        {
            if (_entrando) return;
            Abrir(ModoPartidas.Cargar);
        }

        /// <summary>Flechas / WASD: recorre las velas y, después de la última, el botón de volver.</summary>
        public void Mover(int delta)
        {
            if (!Interactuable || delta == 0) return;
            int total = _ranuras.Count + 1;
            if (_indice < 0 || _indice >= total)
                _indice = delta > 0 ? 0 : total - 1;
            else
                _indice = ((_indice + delta) % total + total) % total;
            Publicar();
        }

        /// <summary>Mouse encima de una vela (0..N-1) o del botón de volver (N).</summary>
        public void Hover(int indice)
        {
            if (!Interactuable || indice < 0 || indice > _ranuras.Count || indice == _indice) return;
            _indice = indice;
            Publicar();
        }

        /// <summary>Espacio / Enter sobre lo seleccionado.</summary>
        public void Confirmar()
        {
            if (_indice != LoadGameViewState.SIN_SELECCION) Activar(_indice);
        }

        /// <summary>Clic en una vela (0..N-1) o en el botón de volver (N).</summary>
        public void Activar(int indice)
        {
            if (!Interactuable || indice < 0 || indice > _ranuras.Count) return;

            if (indice == _ranuras.Count)
            {
                Volver();
                return;
            }

            _indice = indice;
            var ranura = _ranuras[indice];
            bool listo = _modo == ModoPartidas.Reemplazar
                ? _iniciarPartida.Ejecutar(ranura.Numero)
                : _cargarRanura.Ejecutar(ranura.Numero);

            if (listo)
            {
                Entrar();
            }
            else
            {
                Publicar();
                OnRanuraVacia?.Invoke(indice);
            }
        }

        /// <summary>Escape o el botón "Volver".</summary>
        public void Volver()
        {
            if (!Interactuable) return;
            _visible = false;
            _indice = LoadGameViewState.SIN_SELECCION;
            Publicar();
        }

        private bool Interactuable => _visible && !_entrando;

        private void Abrir(ModoPartidas modo)
        {
            _ranuras = _obtenerRanuras.Ejecutar();
            _modo = modo;
            _visible = true;
            _indice = PrimeraVelaEncendida();
            Publicar();
        }

        private void Entrar()
        {
            _entrando = true;
            Publicar();
            OnEntrarAPartida?.Invoke();
        }

        private int PrimeraVelaEncendida()
        {
            for (int i = 0; i < _ranuras.Count; i++)
                if (_ranuras[i] != null && _ranuras[i].TienePartida) return i;
            return LoadGameViewState.SIN_SELECCION;
        }

        private void Publicar()
        {
            bool reemplazar = _modo == ModoPartidas.Reemplazar;
            EstadoActual = new LoadGameViewState(
                _visible,
                _modo,
                _ranuras,
                _indice,
                reemplazar ? "Nueva partida" : "Cargar partida",
                reemplazar
                    ? "Todas las velas están encendidas. Elige cuál reemplazar."
                    : "Elige una vela encendida para continuar.",
                TextoInfo(reemplazar));
            OnStateChanged?.Invoke(EstadoActual);
        }

        private string TextoInfo(bool reemplazar)
        {
            if (_indice < 0 || _indice >= _ranuras.Count) return string.Empty;
            var r = _ranuras[_indice];
            if (r == null || !r.TienePartida)
                return "Vela apagada\nNo hay partida guardada";

            string texto =
                $"Capítulo {r.CapituloActual}\n" +
                $"Completado: {r.PorcentajeCompletado}%\n" +
                $"Tiempo de partida: {ReglasProgreso.FormatearTiempo(r.SegundosJugados)}\n" +
                $"Último guardado: {ReglasRanuras.FormatearFecha(r.GuardadoUnixMs)}";
            return reemplazar ? texto + "\nSe reemplazará por una partida nueva" : texto;
        }
    }
}
