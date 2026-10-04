using System;

namespace UniversalPlatform.Features.ChapterSelect.Domain
{
    public class ValidarEntradaCapituloUseCase
    {
        private readonly CapitulosRepositoryPort _repositorio;

        public ValidarEntradaCapituloUseCase(CapitulosRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public ResultadoEntradaCapitulo Ejecutar(int numeroReloj)
        {
            var capitulo = _repositorio.ObtenerPorNumero(numeroReloj);
            if (capitulo == null) return new ResultadoEntradaCapitulo(EstadoEntradaCapitulo.SinCapitulo);
            if (!capitulo.Disponible) return new ResultadoEntradaCapitulo(EstadoEntradaCapitulo.Bloqueada);
            return new ResultadoEntradaCapitulo(EstadoEntradaCapitulo.Permitida, capitulo);
        }
    }
}
