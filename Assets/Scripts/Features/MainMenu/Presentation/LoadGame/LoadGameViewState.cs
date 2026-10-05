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
        /// 0..N-1 = vela; N = botón "Volver al menú principal"; <see cref="SIN_SELECCION"/> = nada.
        /// </summary>
        public int IndiceSeleccionado { get; }
        public string Titulo { get; }
        public string Subtitulo { get; }
        /// <summary>Texto del recuadro de información; vacío si no hay una vela seleccionada.</summary>
        public string TextoInfo { get; }

        public int IndiceVolver => Ranuras.Count;
        public bool VolverSeleccionado => IndiceSeleccionado == IndiceVolver;
        public bool HayVelaSeleccionada => IndiceSeleccionado >= 0 && IndiceSeleccionado < Ranuras.Count;

        public LoadGameViewState(bool visible, ModoPartidas modo, IReadOnlyList<ResumenRanura> ranuras,
            int indiceSeleccionado, string titulo, string subtitulo, string textoInfo)
        {
            Visible = visible;
            Modo = modo;
            Ranuras = ranuras ?? new List<ResumenRanura>();
            IndiceSeleccionado = indiceSeleccionado;
            Titulo = titulo ?? string.Empty;
            Subtitulo = subtitulo ?? string.Empty;
            TextoInfo = textoInfo ?? string.Empty;
        }
    }
}
