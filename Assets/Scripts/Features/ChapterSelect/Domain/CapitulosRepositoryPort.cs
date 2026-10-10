using System.Collections.Generic;

namespace UniversalPlatform.Features.ChapterSelect.Domain
{
    public interface CapitulosRepositoryPort
    {
        IReadOnlyList<Capitulo> ObtenerCapitulos();

        /// <summary>Devuelve null si ese número del reloj no tiene capítulo.</summary>
        Capitulo ObtenerPorNumero(int numeroReloj);
    }
}
