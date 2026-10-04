using System;

namespace UniversalPlatform.Features.Progress.Domain
{
    public class IniciarNuevaPartidaUseCase
    {
        private readonly ProgresoRepositoryPort _repositorio;

        public IniciarNuevaPartidaUseCase(ProgresoRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public void Ejecutar()
        {
            _repositorio.ReiniciarPartida();
        }
    }
}
