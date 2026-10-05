using System.Collections.Generic;

namespace UniversalPlatform.Features.Progress.Domain
{
    /// <summary>
    /// Acceso a las ranuras de guardado. La "ranura activa" es la partida sobre la que leen y
    /// escriben la selección de capítulo y los capítulos mientras se juega.
    /// </summary>
    public interface RanurasRepositoryPort
    {
        /// <summary>Un resumen por ranura, ordenados de la 1 a la última. Nunca devuelve null.</summary>
        IReadOnlyList<ResumenRanura> ObtenerResumenes();

        /// <summary>Deja la ranura como activa, sin tocar lo guardado (cargar partida).</summary>
        void SeleccionarRanura(int numero);

        /// <summary>Crea una partida vacía en la ranura (reemplaza la que hubiera) y la deja activa.</summary>
        void IniciarPartidaEn(int numero);
    }
}
