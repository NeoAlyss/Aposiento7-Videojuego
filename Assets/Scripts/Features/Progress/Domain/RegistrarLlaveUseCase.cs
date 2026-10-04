using System;

namespace UniversalPlatform.Features.Progress.Domain
{
    public class RegistrarLlaveUseCase
    {
        private readonly ProgresoRepositoryPort _repositorio;

        public RegistrarLlaveUseCase(ProgresoRepositoryPort repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public ProgresoCapitulo Ejecutar(string idCapitulo, int cantidad = 1)
        {
            var actual = _repositorio.ObtenerProgreso(idCapitulo);
            var nuevo = actual.ConLlaves(ReglasProgreso.SumarLlaves(actual.Llaves, cantidad));
            _repositorio.GuardarProgreso(nuevo);
            return nuevo;
        }
    }
}
