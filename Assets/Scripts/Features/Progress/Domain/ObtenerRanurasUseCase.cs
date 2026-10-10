using System;
using System.Collections.Generic;

namespace UniversalPlatform.Features.Progress.Domain
{
    public class ObtenerRanurasUseCase
    {
        private readonly RanurasRepositoryPort _repositorio;

        public ObtenerRanurasUseCase(RanurasRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public IReadOnlyList<ResumenRanura> Ejecutar()
        {
            return _repositorio.ObtenerResumenes();
        }
    }
}
