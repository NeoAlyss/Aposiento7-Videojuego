namespace UniversalPlatform.Features.Progress.Presentation.ChapterTimer
{
    public class ChapterTimerViewState
    {
        public string IdCapitulo { get; }
        public int Llaves { get; }
        /// <summary>Total jugado en el capítulo, sumando todas las veces (lo que queda guardado).</summary>
        public float SegundosJugados { get; }
        /// <summary>Lo que va de este intento: parte en 0 cada vez que se entra al capítulo.</summary>
        public float SegundosIntento { get; }
        /// <summary>El tiempo de este intento, listo para mostrar.</summary>
        public string TiempoFormateado { get; }
        public bool Completado { get; }

        public ChapterTimerViewState(
            string idCapitulo,
            int llaves,
            float segundosJugados,
            string tiempoFormateado,
            bool completado,
            float segundosIntento = 0f)
        {
            IdCapitulo = idCapitulo;
            Llaves = llaves;
            SegundosJugados = segundosJugados;
            TiempoFormateado = tiempoFormateado;
            Completado = completado;
            SegundosIntento = segundosIntento;
        }
    }
}
