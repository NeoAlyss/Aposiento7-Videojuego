using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Data
{
    /// <summary>
    /// Ranura de guardado en uso durante esta sesión de juego. Es estática para que sobreviva al
    /// cambio de escena (menú → selección de capítulo → capítulo). Al abrir el juego vale 1.
    /// </summary>
    public static class RanuraActiva
    {
        private static int _numero = 1;

        public static int Numero
        {
            get => _numero;
            set => _numero = ReglasRanuras.EsNumeroValido(value) ? value : 1;
        }
    }
}
