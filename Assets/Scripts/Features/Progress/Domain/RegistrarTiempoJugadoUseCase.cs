using System;

namespace UniversalPlatform.Features.Progress.Domain
{
    public class RegistrarTiempoJugadoUseCase
    {
        private readonly ProgresoRepositoryPort _repositorio;

        public RegistrarTiempoJugadoUseCase(ProgresoRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public ProgresoCapitulo Ejecutar(string idCapitulo, float segundos)
        {
            var actual = _repositorio.ObtenerProgreso(idCapitulo);
            var nuevo = actual.ConSegundos(ReglasProgreso.SumarTiempo(actual.SegundosJugados, segundos));
            _repositorio.GuardarProgreso(nuevo);
            return nuevo;
        }
    }
}
