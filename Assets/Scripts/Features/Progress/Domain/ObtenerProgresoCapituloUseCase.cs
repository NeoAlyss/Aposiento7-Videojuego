using System;

namespace UniversalPlatform.Features.Progress.Domain
{
    public class ObtenerProgresoCapituloUseCase
    {
        private readonly ProgresoRepositoryPort _repositorio;

        public ObtenerProgresoCapituloUseCase(ProgresoRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public ProgresoCapitulo Ejecutar(string idCapitulo)
        {
            return _repositorio.ObtenerProgreso(idCapitulo);
        }
    }
}
