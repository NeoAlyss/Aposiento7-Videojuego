using UnityEngine;
using UniversalPlatform.Features.Progress.Data;
using UniversalPlatform.Features.Progress.Datasources;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.DI
{
    /// <summary>
    /// Arma los repositorios de progreso. Lo reutilizan los Installers de MainMenu, ChapterSelect y
    /// de la escena de cada capítulo, para que todos lean/escriban los mismos archivos JSON.
    /// </summary>
    public static class ProgresoCompositionRoot
    {
        /// <summary>Progreso de la ranura activa (la partida elegida en el menú).</summary>
        public static ProgresoRepositoryPort CrearRepositorio(string directorioBase = null)
        {
            var defaultDataSource = new ProgresoDefaultDataSource();
            var localDataSource = new JsonFileProgresoLocalDataSource(
                directorioBase ?? Application.persistentDataPath,
                ArchivosRanura.Nombre(RanuraActiva.Numero));
            return new ProgresoDataRepository(defaultDataSource, localDataSource);
        }

        /// <summary>Ranuras de guardado (las velas de "Cargar partida").</summary>
        public static RanurasRepositoryPort CrearRepositorioRanuras(string directorioBase = null)
        {
            return new RanurasDataRepository(
                new JsonFileRanurasLocalDataSource(directorioBase ?? Application.persistentDataPath));
        }
    }
}
