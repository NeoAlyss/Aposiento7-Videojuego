namespace UniversalPlatform.Features.ChapterSelect.Domain
{
    public static class ReglasCapitulo
    {
        public static string NombreDificultad(DificultadCapitulo dificultad)
        {
            return dificultad switch
            {
                DificultadCapitulo.Facil => "Fácil",
                DificultadCapitulo.Media => "Media",
                DificultadCapitulo.Dificil => "Difícil",
                _ => "Desconocida"
            };
        }

        public static string TituloSinCapitulo(int numeroReloj)
        {
            return $"Hora {numeroReloj}";
        }
    }
}
