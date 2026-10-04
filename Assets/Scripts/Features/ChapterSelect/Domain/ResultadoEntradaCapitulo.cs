namespace UniversalPlatform.Features.ChapterSelect.Domain
{
    public sealed class ResultadoEntradaCapitulo
    {
        public EstadoEntradaCapitulo Estado { get; }

        /// <summary>Solo tiene valor cuando el estado es Permitida.</summary>
        public Capitulo Capitulo { get; }

        public ResultadoEntradaCapitulo(EstadoEntradaCapitulo estado, Capitulo capitulo = null)
        {
            Estado = estado;
            Capitulo = capitulo;
        }
    }
}
