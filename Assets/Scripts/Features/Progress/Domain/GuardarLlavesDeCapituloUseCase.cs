using System;

namespace UniversalPlatform.Features.Progress.Domain
{
    /// <summary>
    /// Deja registrada la mejor cantidad de llaves conseguida en un capítulo. A diferencia de
    /// <see cref="RegistrarLlaveUseCase"/> no suma: repetir un capítulo no infla el contador.
    /// </summary>
    public class GuardarLlavesDeCapituloUseCase
    {
        private readonly ProgresoRepositoryPort _repositorio;

        public GuardarLlavesDeCapituloUseCase(ProgresoRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public ProgresoCapitulo Ejecutar(string idCapitulo, int llaves)
        {
            var actual = _repositorio.ObtenerProgreso(idCapitulo);
            if (llaves <= actual.Llaves) return actual;
            var nuevo = actual.ConLlaves(llaves);
            _repositorio.GuardarProgreso(nuevo);
            return nuevo;
        }
    }
}
