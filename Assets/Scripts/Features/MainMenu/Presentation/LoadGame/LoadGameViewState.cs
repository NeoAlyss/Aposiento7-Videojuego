using System.Collections.Generic;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.MainMenu.Presentation.LoadGame
{
    /// <summary>Para qué se abrió la pantalla de velas.</summary>
    public enum ModoPartidas
    {
        /// <summary>Elegir una partida guardada para continuarla.</summary>
        Cargar,
        /// <summary>Todas las ranuras están ocupadas: elegir cuál reemplazar con la partida nueva.</summary>
        Reemplazar
    }

    public class LoadGameViewState
    {
        public const int SIN_SELECCION = -1;

        public bool Visible { get; }
        public ModoPartidas Modo { get; }
        /// <summary>Una por vela, en orden (ranura 1 a la izquierda).</summary>
        public IReadOnlyList<ResumenRanura> Ranuras { get; }
        /// <summary>
        /// 0..N-1 = vela; N = botón "Volver"; N+1 = botón "Borrar partida"; <see cref="SIN_SELECCION"/> = nada.
        /// </summary>
        public int IndiceSeleccionado { get; }
        public string Titulo { get; }
        public string Subtitulo { get; }
        /// <summary>Texto del recuadro de información; vacío si no hay una vela seleccionada.</summary>
        public string TextoInfo { get; }
        /// <summary>Hay una vela encendida seleccionada: se puede borrar su partida.</summary>
        public bool PuedeBorrar { get; }
        /// <summary>Se pidió borrar una vez: la siguiente confirmación la borra.</summary>
        public bool ConfirmandoBorrado { get; }

        /// <summary>
        /// Vela de la que se muestra la información y que se borraría: la seleccionada o, con el foco
        /// en "Borrar partida", la elegida antes de bajar al botón.
        /// </summary>
        public int IndiceVela { get; }

        public int IndiceVolver => Ranuras.Count;
        public int IndiceBorrar => Ranuras.Count + 1;
        public bool VolverSeleccionado => IndiceSeleccionado == IndiceVolver;
        public bool BorrarSeleccionado => IndiceSeleccionado == IndiceBorrar;
        public bool HayVelaSeleccionada => IndiceVela >= 0 && IndiceVela < Ranuras.Count;

        public LoadGameViewState(bool visible, ModoPartidas modo, IReadOnlyList<ResumenRanura> ranuras,
            int indiceSeleccionado, string titulo, string subtitulo, string textoInfo,
            bool puedeBorrar = false, bool confirmandoBorrado = false, int indiceVela = int.MinValue)
        {
            Visible = visible;
            Modo = modo;
            Ranuras = ranuras ?? new List<ResumenRanura>();
            IndiceSeleccionado = indiceSeleccionado;
            Titulo = titulo ?? string.Empty;
            Subtitulo = subtitulo ?? string.Empty;
            TextoInfo = textoInfo ?? string.Empty;
            PuedeBorrar = puedeBorrar;
            ConfirmandoBorrado = confirmandoBorrado;
            // Sin dato explícito, la vela es la selección (si es una vela).
            IndiceVela = indiceVela != int.MinValue ? indiceVela
                : indiceSeleccionado >= 0 && indiceSeleccionado < Ranuras.Count ? indiceSeleccionado : SIN_SELECCION;
        }
    }
}
