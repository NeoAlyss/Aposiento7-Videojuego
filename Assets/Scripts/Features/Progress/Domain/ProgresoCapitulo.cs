namespace UniversalPlatform.Features.Progress.Domain
{
    /// <summary>
    /// Progreso inmutable de un capítulo: llaves conseguidas y tiempo de juego (parte en 0).
    /// </summary>
    public sealed class ProgresoCapitulo
    {
        public string IdCapitulo { get; }
        public int Llaves { get; }
        public float SegundosJugados { get; }
        public bool Completado { get; }

        public ProgresoCapitulo(string idCapitulo, int llaves, float segundosJugados, bool completado)
        {
            IdCapitulo = idCapitulo;
            Llaves = llaves;
            SegundosJugados = segundosJugados;
            Completado = completado;
        }

        public static ProgresoCapitulo Nuevo(string idCapitulo) =>
            new ProgresoCapitulo(idCapitulo, 0, ReglasProgreso.SEGUNDOS_INICIALES, false);

        public ProgresoCapitulo ConLlaves(int llaves) =>
            new ProgresoCapitulo(IdCapitulo, llaves, SegundosJugados, Completado);

        public ProgresoCapitulo ConSegundos(float segundos) =>
            new ProgresoCapitulo(IdCapitulo, Llaves, segundos, Completado);

        public ProgresoCapitulo ConCompletado(bool completado) =>
            new ProgresoCapitulo(IdCapitulo, Llaves, SegundosJugados, completado);
    }
}
