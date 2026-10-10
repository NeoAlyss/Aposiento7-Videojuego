namespace UniversalPlatform.Features.ChapterSelect.Domain
{
    /// <summary>Textos ya listos para mostrar en los boxes de información.</summary>
    public sealed class DetalleCapitulo
    {
        public int NumeroReloj { get; }
        public string Titulo { get; }
        public string Dificultad { get; }
        public string Llaves { get; }
        public string Tiempo { get; }
        public bool Bloqueado { get; }

        public DetalleCapitulo(int numeroReloj, string titulo, string dificultad, string llaves, string tiempo, bool bloqueado)
        {
            NumeroReloj = numeroReloj;
            Titulo = titulo;
            Dificultad = dificultad;
            Llaves = llaves;
            Tiempo = tiempo;
            Bloqueado = bloqueado;
        }
    }
}
