using System;

namespace UniversalPlatform.Features.Progress.Domain
{
    public class HayPartidaGuardadaUseCase
    {
        private readonly ProgresoRepositoryPort _repositorio;

        public HayPartidaGuardadaUseCase(ProgresoRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public bool Ejecutar()
        {
            return _repositorio.HayPartidaGuardada();
        }
    }
}
