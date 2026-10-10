namespace UniversalPlatform.Features.ChapterSelect.Domain
{
    /// <summary>Un capítulo = una hora del reloj (1 a 12).</summary>
    public sealed class Capitulo
    {
        public string Id { get; }
        public int NumeroReloj { get; }
        public string Titulo { get; }
        public DificultadCapitulo Dificultad { get; }
        public int TotalLlaves { get; }
        public bool Disponible { get; }
        public string NombreEscena { get; }

        public Capitulo(
            string id,
            int numeroReloj,
            string titulo,
            DificultadCapitulo dificultad,
            int totalLlaves,
            bool disponible,
            string nombreEscena)
        {
            Id = id;
            NumeroReloj = numeroReloj;
            Titulo = titulo;
            Dificultad = dificultad;
            TotalLlaves = totalLlaves;
            Disponible = disponible;
            NombreEscena = nombreEscena;
        }
    }
}
