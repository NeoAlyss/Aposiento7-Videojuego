using System;
using System.Collections.Generic;

namespace UniversalPlatform.Features.ChapterSelect.Domain
{
    public class ObtenerCapitulosUseCase
    {
        private readonly CapitulosRepositoryPort _repositorio;

        public ObtenerCapitulosUseCase(CapitulosRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public IReadOnlyList<Capitulo> Ejecutar()
        {
            return _repositorio.ObtenerCapitulos();
        }
    }
}
