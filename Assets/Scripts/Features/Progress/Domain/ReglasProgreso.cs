using System;

namespace UniversalPlatform.Features.Progress.Domain
{
    public static class ReglasProgreso
    {
        public const float SEGUNDOS_INICIALES = 0f;

        /// <summary>Formato HH:MM:SS. Valores negativos o inválidos se muestran como 00:00:00.</summary>
        public static string FormatearTiempo(float segundos)
        {
            if (float.IsNaN(segundos) || segundos < 0f) segundos = 0f;
            int total = (int)Math.Floor(segundos);
            int horas = total / 3600;
            int minutos = total / 60 % 60;
            int seg = total % 60;
            return $"{horas:00}:{minutos:00}:{seg:00}";
        }

        public static string FormatearLlaves(int llaves, int total)
        {
            return $"{Math.Max(0, llaves)} / {Math.Max(0, total)}";
        }

        /// <summary>Suma tiempo ignorando deltas negativos, cero o inválidos.</summary>
        public static float SumarTiempo(float actual, float delta)
        {
            if (float.IsNaN(delta) || delta <= 0f) return actual;
            return actual + delta;
        }

        /// <summary>Suma llaves ignorando cantidades no positivas.</summary>
        public static int SumarLlaves(int actual, int cantidad)
        {
            return cantidad <= 0 ? actual : actual + cantidad;
        }
    }
}
