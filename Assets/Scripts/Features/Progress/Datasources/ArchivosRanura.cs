namespace UniversalPlatform.Features.Progress.Datasources
{
    /// <summary>Nombre del archivo JSON de cada ranura de guardado.</summary>
    public static class ArchivosRanura
    {
        /// <summary>
        /// La ranura 1 conserva el nombre del guardado único original, para que una partida creada
        /// antes de que existieran las ranuras siga apareciendo (en la primera vela).
        /// </summary>
        public const string NOMBRE_RANURA_1 = "progreso_partida.json";

        public static string Nombre(int numero)
        {
            return numero <= 1 ? NOMBRE_RANURA_1 : $"progreso_partida_{numero}.json";
        }
    }
}
