using System;

namespace UniversalPlatform.Features.Progress.Domain
{
    /// <summary>Cargar partida: deja activa una ranura que ya tiene partida guardada.</summary>
    public class CargarRanuraUseCase
    {
        private readonly RanurasRepositoryPort _repositorio;

        public CargarRanuraUseCase(RanurasRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        /// <returns>false si el número no es válido o la ranura está vacía (no se cambia nada).</returns>
        public bool Ejecutar(int numero)
        {
            if (!ReglasRanuras.EsNumeroValido(numero)) return false;
            foreach (var r in _repositorio.ObtenerResumenes())
            {
                if (r == null || r.Numero != numero) continue;
                if (!r.TienePartida) return false;
                _repositorio.SeleccionarRanura(numero);
                return true;
            }
            return false;
        }
    }
}
