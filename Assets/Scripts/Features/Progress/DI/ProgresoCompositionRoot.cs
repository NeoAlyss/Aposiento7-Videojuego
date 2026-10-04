using UnityEngine;
using UniversalPlatform.Features.Progress.Data;
using UniversalPlatform.Features.Progress.Datasources;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.DI
{
    /// <summary>
    /// Arma el repositorio de progreso. Lo reutilizan los Installers de MainMenu, ChapterSelect y
    /// de la escena de cada capítulo, para que todos lean/escriban el mismo archivo JSON.
    /// </summary>
    public static class ProgresoCompositionRoot
    {
        public static ProgresoRepositoryPort CrearRepositorio(string directorioBase = null)
        {
            var defaultDataSource = new ProgresoDefaultDataSource();
            var localDataSource = new JsonFileProgresoLocalDataSource(directorioBase ?? Application.persistentDataPath);
            return new ProgresoDataRepository(defaultDataSource, localDataSource);
        }
    }
}
