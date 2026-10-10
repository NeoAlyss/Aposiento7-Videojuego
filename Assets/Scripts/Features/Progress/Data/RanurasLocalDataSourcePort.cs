using System.Collections.Generic;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Data
{
    /// <summary>Fuente persistente de las ranuras de guardado (un archivo por ranura).</summary>
    public interface RanurasLocalDataSourcePort
    {
        bool Existe(int numero);

        /// <summary>Capítulos guardados en la ranura; null si no hay archivo o está dañado.</summary>
        IReadOnlyList<ProgresoCapitulo> LeerCapitulos(int numero);

        /// <summary>Momento del último guardado en milisegundos Unix; 0 si no se conoce.</summary>
        long LeerGuardadoUnixMs(int numero);

        /// <summary>Deja en la ranura una partida vacía, reemplazando la que hubiera.</summary>
        void CrearVacia(int numero);

        /// <summary>Elimina el archivo de la ranura.</summary>
        void Borrar(int numero);
    }
}
