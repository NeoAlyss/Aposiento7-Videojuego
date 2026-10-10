using System.Collections.Generic;
using UniversalPlatform.Features.ChapterSelect.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Data
{
    /// <summary>Catálogo de capítulos por defecto (definido en código; sin persistencia).</summary>
    public interface CapitulosDefaultDataSourcePort
    {
        IReadOnlyList<Capitulo> LeerCapitulos();
    }
}
