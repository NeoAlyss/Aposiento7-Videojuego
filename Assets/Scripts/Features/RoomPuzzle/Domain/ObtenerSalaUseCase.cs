using System;

namespace UniversalPlatform.Features.RoomPuzzle.Domain
{
    public class ObtenerSalaUseCase
    {
        private readonly SalasRepositoryPort _repositorio;

        public ObtenerSalaUseCase(SalasRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public DefinicionSala Ejecutar(string idCapitulo) => _repositorio.Obtener(idCapitulo);
    }
}
