using System;

namespace UniversalPlatform.Features.Progress.Domain
{
    public class CompletarCapituloUseCase
    {
        private readonly ProgresoRepositoryPort _repositorio;

        public CompletarCapituloUseCase(ProgresoRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public ProgresoCapitulo Ejecutar(string idCapitulo)
        {
            var nuevo = _repositorio.ObtenerProgreso(idCapitulo).ConCompletado(true);
            _repositorio.GuardarProgreso(nuevo);
            return nuevo;
        }
    }
}
