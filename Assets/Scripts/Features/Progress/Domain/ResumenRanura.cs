namespace UniversalPlatform.Features.Progress.Domain
{
    /// <summary>
    /// Lo que se muestra de una ranura de guardado en la pantalla "Cargar partida" (una vela por
    /// ranura). Inmutable. Si <see cref="TienePartida"/> es false, el resto de los datos no aplica.
    /// </summary>
    public sealed class ResumenRanura
    {
        /// <summary>Número de la ranura, de 1 a <see cref="ReglasRanuras.CANTIDAD"/>.</summary>
        public int Numero { get; }
        public bool TienePartida { get; }
        /// <summary>Capítulo en el que va la partida (1 = primero).</summary>
        public int CapituloActual { get; }
        public int PorcentajeCompletado { get; }
        /// <summary>Tiempo total jugado, sumando todos los capítulos.</summary>
        public float SegundosJugados { get; }
        /// <summary>Momento del último guardado, en milisegundos Unix (UTC). 0 si no se conoce.</summary>
        public long GuardadoUnixMs { get; }

        public ResumenRanura(int numero, bool tienePartida, int capituloActual, int porcentajeCompletado,
            float segundosJugados, long guardadoUnixMs)
        {
            Numero = numero;
            TienePartida = tienePartida;
            CapituloActual = capituloActual;
            PorcentajeCompletado = porcentajeCompletado;
            SegundosJugados = segundosJugados;
            GuardadoUnixMs = guardadoUnixMs;
        }

        public static ResumenRanura Vacia(int numero) => new ResumenRanura(numero, false, 0, 0, 0f, 0L);
    }
}
