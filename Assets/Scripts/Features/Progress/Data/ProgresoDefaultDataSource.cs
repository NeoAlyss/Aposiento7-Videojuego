using System.Collections.Generic;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Data
{
    public class ProgresoDefaultDataSource : ProgresoDefaultDataSourcePort
    {
        private readonly Dictionary<string, ProgresoCapitulo> _progresos = new Dictionary<string, ProgresoCapitulo>();

        public ProgresoCapitulo Leer(string idCapitulo)
        {
            return _progresos.TryGetValue(idCapitulo, out var progreso)
                ? progreso
                : ProgresoCapitulo.Nuevo(idCapitulo);
        }

        public void Guardar(ProgresoCapitulo progreso)
        {
            _progresos[progreso.IdCapitulo] = progreso;
        }

        public IReadOnlyCollection<ProgresoCapitulo> LeerTodos() => _progresos.Values;

        public void Limpiar() => _progresos.Clear();
    }
}
