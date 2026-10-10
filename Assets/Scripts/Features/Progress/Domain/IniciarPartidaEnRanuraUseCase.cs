using System;

namespace UniversalPlatform.Features.Progress.Domain
{
    /// <summary>Nueva partida en una ranura concreta: borra lo que tuviera y la deja activa.</summary>
    public class IniciarPartidaEnRanuraUseCase
    {
        private readonly RanurasRepositoryPort _repositorio;

        public IniciarPartidaEnRanuraUseCase(RanurasRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        /// <returns>false si el número de ranura no es válido.</returns>
        public bool Ejecutar(int numero)
        {
            if (!ReglasRanuras.EsNumeroValido(numero)) return false;
            _repositorio.IniciarPartidaEn(numero);
            return true;
        }
    }
}
