namespace UniversalPlatform.Features.Progress.Presentation.ChapterTimer
{
    public class ChapterTimerViewState
    {
        public string IdCapitulo { get; }
        public int Llaves { get; }
        public float SegundosJugados { get; }
        public string TiempoFormateado { get; }
        public bool Completado { get; }

        public ChapterTimerViewState(
            string idCapitulo,
            int llaves,
            float segundosJugados,
            string tiempoFormateado,
            bool completado)
        {
            IdCapitulo = idCapitulo;
            Llaves = llaves;
            SegundosJugados = segundosJugados;
            TiempoFormateado = tiempoFormateado;
            Completado = completado;
        }
    }
}
