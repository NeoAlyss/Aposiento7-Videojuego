using System.Collections.Generic;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Data
{
    /// <summary>Fuente persistente (archivo). Devuelve null en LeerTodo si no hay archivo o está dañado.</summary>
    public interface ProgresoLocalDataSourcePort
    {
        IReadOnlyList<ProgresoCapitulo> LeerTodo();
        void GuardarTodo(IReadOnlyCollection<ProgresoCapitulo> progresos);
        bool Existe();
    }
}
