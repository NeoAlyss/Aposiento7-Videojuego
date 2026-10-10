using System;

namespace UniversalPlatform.Features.Progress.Domain
{
    /// <summary>Borra la partida de una ranura. No se puede deshacer.</summary>
    public class BorrarRanuraUseCase
    {
        private readonly RanurasRepositoryPort _repositorio;

        public BorrarRanuraUseCase(RanurasRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        /// <returns>false si el número no es válido o la ranura ya estaba vacía.</returns>
        public bool Ejecutar(int numero)
        {
            if (!ReglasRanuras.EsNumeroValido(numero)) return false;
            foreach (var r in _repositorio.ObtenerResumenes())
            {
                if (r == null || r.Numero != numero) continue;
                if (!r.TienePartida) return false;
                _repositorio.BorrarPartida(numero);
                return true;
            }
            return false;
        }
    }
}
