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
        private readonly BorrarRanuraUseCase _borrarRanura;

        private IReadOnlyList<ResumenRanura> _ranuras = new List<ResumenRanura>();
        private ModoPartidas _modo = ModoPartidas.Cargar;
        private int _indice = LoadGameViewState.SIN_SELECCION;
        private bool _visible;
        private bool _entrando;
        private int _confirmandoBorrado = LoadGameViewState.SIN_SELECCION;
        // Última vela elegida: es la que borra el botón "Borrar partida" cuando el foco está en él.
        private int _vela = LoadGameViewState.SIN_SELECCION;

        public LoadGameViewState EstadoActual { get; private set; }

        public event Action<LoadGameViewState> OnStateChanged;
        /// <summary>Ya hay una ranura activa lista para jugar: toca ir a la selección de capítulo.</summary>
        public event Action OnEntrarAPartida;
        /// <summary>Se intentó cargar una vela apagada (índice de la vela).</summary>
        public event Action<int> OnRanuraVacia;
        /// <summary>Se borró la partida de una vela (índice de la vela).</summary>
        public event Action<int> OnRanuraBorrada;

        public LoadGameViewModel(
            ObtenerRanurasUseCase obtenerRanuras,
            CargarRanuraUseCase cargarRanura,
            IniciarPartidaEnRanuraUseCase iniciarPartida,
            BorrarRanuraUseCase borrarRanura = null)
        {
            _obtenerRanuras = obtenerRanuras ?? throw new ArgumentNullException(nameof(obtenerRanuras));
            _cargarRanura = cargarRanura ?? throw new ArgumentNullException(nameof(cargarRanura));
            _iniciarPartida = iniciarPartida ?? throw new ArgumentNullException(nameof(iniciarPartida));
            _borrarRanura = borrarRanura;
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

        private int IndiceVolver => _ranuras.Count;
        private int IndiceBorrar => _ranuras.Count + 1;

        /// <summary>
        /// Izquierda / derecha: recorre las velas, el botón de volver y, si la vela elegida tiene
        /// partida, el de borrar (en ese orden, dando la vuelta).
        /// </summary>
        public void Mover(int delta)
        {
            if (!Interactuable || delta == 0) return;
            var orden = new List<int>();
            for (int i = 0; i < _ranuras.Count; i++) orden.Add(i);
            orden.Add(IndiceVolver);
            if (PuedeBorrarVela) orden.Add(IndiceBorrar);

            int pos = orden.IndexOf(_indice);
            if (pos < 0) pos = delta > 0 ? 0 : orden.Count - 1;
            else pos = ((pos + Math.Sign(delta)) % orden.Count + orden.Count) % orden.Count;
            Seleccionar(orden[pos]);
        }

        /// <summary>
        /// Arriba / abajo: de las velas baja a los botones (a "Borrar partida" si la vela tiene
        /// partida; si no, a "Volver") y de los botones sube a la vela elegida.
        /// </summary>
        public void MoverVertical(int delta)
        {
            if (!Interactuable || delta == 0) return;
            bool enVela = _indice >= 0 && _indice < _ranuras.Count;
            if (delta > 0)
            {
                if (enVela) Seleccionar(PuedeBorrarVela ? IndiceBorrar : IndiceVolver);
                else if (_indice == LoadGameViewState.SIN_SELECCION) Seleccionar(IndiceVolver);
            }
            else if (!enVela)
            {
                Seleccionar(_vela >= 0 && _vela < _ranuras.Count ? _vela : 0);
            }
        }

        /// <summary>Mouse encima de una vela (0..N-1), del botón de volver (N) o del de borrar (N+1).</summary>
        public void Hover(int indice)
        {
            if (!Interactuable || indice < 0 || indice > IndiceBorrar || indice == _indice) return;
            if (indice == IndiceBorrar && !PuedeBorrarVela) return;
            Seleccionar(indice);
        }

        private void Seleccionar(int indice)
        {
            _indice = indice;
            bool esVela = indice >= 0 && indice < _ranuras.Count;
            // Cambiar de vela (o ir a "Volver") cancela una confirmación de borrado pendiente;
            // bajar de la vela al botón de borrar, no.
            if (esVela && indice != _vela || indice == IndiceVolver)
                _confirmandoBorrado = LoadGameViewState.SIN_SELECCION;
            if (esVela) _vela = indice;
            Publicar();
        }

        /// <summary>Espacio / Enter sobre lo seleccionado.</summary>
        public void Confirmar()
        {
            if (_indice != LoadGameViewState.SIN_SELECCION) Activar(_indice);
        }

        /// <summary>Clic en una vela (0..N-1), en el botón de volver (N) o en el de borrar (N+1).</summary>
        public void Activar(int indice)
        {
            if (!Interactuable || indice < 0 || indice > IndiceBorrar) return;

            if (indice == IndiceVolver)
            {
                Volver();
                return;
            }
            if (indice == IndiceBorrar)
            {
                SolicitarBorrado();
                return;
            }

            _indice = indice;
            _vela = indice;
            _confirmandoBorrado = LoadGameViewState.SIN_SELECCION;
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
            _vela = LoadGameViewState.SIN_SELECCION;
            _confirmandoBorrado = LoadGameViewState.SIN_SELECCION;
            Publicar();
        }

        /// <summary>
        /// Supr o el botón "Borrar partida": la primera vez pide confirmación; la segunda, sobre la
        /// misma vela, borra la partida y la vela se apaga.
        /// </summary>
        public void SolicitarBorrado()
        {
            if (!Interactuable || !PuedeBorrarVela) return;

            if (_confirmandoBorrado != _vela)
            {
                _confirmandoBorrado = _vela;
                Publicar();
                return;
            }

            int borrada = _vela;
            _confirmandoBorrado = LoadGameViewState.SIN_SELECCION;
            if (_borrarRanura.Ejecutar(_ranuras[borrada].Numero))
            {
                _ranuras = _obtenerRanuras.Ejecutar();
                // La vela se apagó: el botón de borrar desaparece, así que el foco vuelve a la vela.
                if (_indice == IndiceBorrar) _indice = borrada;
                Publicar();
                OnRanuraBorrada?.Invoke(borrada);
            }
            else
            {
                Publicar();
            }
        }

        /// <summary>La vela elegida: la seleccionada o, con el foco en los botones, la última elegida.</summary>
        private int VelaActual =>
            _indice >= 0 && _indice <= IndiceBorrar ? _vela : LoadGameViewState.SIN_SELECCION;

        private bool PuedeBorrarVela =>
            _borrarRanura != null && VelaActual >= 0 && VelaActual < _ranuras.Count &&
            _ranuras[VelaActual] != null && _ranuras[VelaActual].TienePartida;

        private bool Interactuable => _visible && !_entrando;

        private void Abrir(ModoPartidas modo)
        {
            _ranuras = _obtenerRanuras.Ejecutar();
            _modo = modo;
            _visible = true;
            _confirmandoBorrado = LoadGameViewState.SIN_SELECCION;
            _indice = PrimeraVelaEncendida();
            _vela = _indice;
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
                TextoInfo(reemplazar),
                PuedeBorrarVela,
                _confirmandoBorrado != LoadGameViewState.SIN_SELECCION && _confirmandoBorrado == VelaActual,
                VelaActual);
            OnStateChanged?.Invoke(EstadoActual);
        }

        private string TextoInfo(bool reemplazar)
        {
            int vela = VelaActual;
            if (vela < 0 || vela >= _ranuras.Count) return string.Empty;
            var r = _ranuras[vela];
            if (r == null || !r.TienePartida)
                return "Vela apagada\nNo hay partida guardada";
            if (_confirmandoBorrado == vela)
                return "¿Borrar la partida de esta vela?\nNo se puede deshacer.\nVuelve a pulsar «Borrar partida» (o Supr) para confirmar.";

            string texto =
                $"Capítulo {r.CapituloActual}\n" +
                $"Completado: {r.PorcentajeCompletado}%\n" +
                $"Tiempo de partida: {ReglasProgreso.FormatearTiempo(r.SegundosJugados)}\n" +
                $"Último guardado: {ReglasRanuras.FormatearFecha(r.GuardadoUnixMs)}";
            return reemplazar ? texto + "\nSe reemplazará por una partida nueva" : texto;
        }
    }
}
