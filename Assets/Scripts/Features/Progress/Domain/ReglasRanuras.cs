using System;
using System.Collections.Generic;

namespace UniversalPlatform.Features.Progress.Domain
{
    /// <summary>Reglas de las ranuras de guardado (las velas de "Cargar partida"). Sin Unity.</summary>
    public static class ReglasRanuras
    {
        /// <summary>Cantidad de ranuras de guardado = cantidad de velas.</summary>
        public const int CANTIDAD = 3;
        public const int SIN_RANURA = 0;
        public const int TOTAL_CAPITULOS = 12;

        /// <summary>
        /// PROVISIONAL: porcentaje de completado que se muestra en toda partida guardada. Cámbialo aquí,
        /// o reemplaza <see cref="CalcularPorcentaje"/> cuando existan los capítulos y se pueda medir
        /// el avance real.
        /// </summary>
        public const int PORCENTAJE_PROVISIONAL = 10;

        public static bool EsNumeroValido(int numero) => numero >= 1 && numero <= CANTIDAD;

        /// <summary>Porcentaje de completado de una partida. Por ahora es un valor fijo.</summary>
        public static int CalcularPorcentaje(IReadOnlyCollection<ProgresoCapitulo> capitulos)
        {
            return PORCENTAJE_PROVISIONAL;
        }

        /// <summary>
        /// Capítulo en el que va la partida: el siguiente al último completado, sin pasarse del
        /// último. Una partida recién creada va en el capítulo 1.
        /// </summary>
        public static int CalcularCapituloActual(IReadOnlyCollection<ProgresoCapitulo> capitulos)
        {
            int completados = 0;
            if (capitulos != null)
                foreach (var c in capitulos)
                    if (c != null && c.Completado) completados++;
            return Math.Min(completados + 1, TOTAL_CAPITULOS);
        }

        public static float SumarSegundos(IReadOnlyCollection<ProgresoCapitulo> capitulos)
        {
            float total = 0f;
            if (capitulos != null)
                foreach (var c in capitulos)
                    if (c != null) total = ReglasProgreso.SumarTiempo(total, c.SegundosJugados);
            return total;
        }

        /// <summary>Primera ranura sin partida, o <see cref="SIN_RANURA"/> si todas están ocupadas.</summary>
        public static int PrimeraLibre(IReadOnlyList<ResumenRanura> ranuras)
        {
            if (ranuras == null) return SIN_RANURA;
            foreach (var r in ranuras)
                if (r != null && !r.TienePartida) return r.Numero;
            return SIN_RANURA;
        }

        /// <summary>Fecha y hora local del guardado, como "04/10/2026 22:15". "—" si no se conoce.</summary>
        public static string FormatearFecha(long unixMs)
        {
            if (unixMs <= 0) return "—";
            try
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(unixMs).ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            }
            catch (ArgumentOutOfRangeException)
            {
                return "—";
            }
        }
    }
}
