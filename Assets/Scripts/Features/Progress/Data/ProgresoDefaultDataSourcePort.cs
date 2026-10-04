using System.Collections.Generic;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Data
{
    /// <summary>Fuente en memoria: mantiene el progreso vivo de la sesión.</summary>
    public interface ProgresoDefaultDataSourcePort
    {
        ProgresoCapitulo Leer(string idCapitulo);
        void Guardar(ProgresoCapitulo progreso);
        IReadOnlyCollection<ProgresoCapitulo> LeerTodos();
        void Limpiar();
    }
}
