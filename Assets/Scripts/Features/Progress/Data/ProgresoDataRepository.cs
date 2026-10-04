using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Data
{
    public class ProgresoDataRepository : ProgresoRepositoryPort
    {
        private readonly ProgresoDefaultDataSourcePort _defaultDataSource;
        private readonly ProgresoLocalDataSourcePort _localDataSource;
        private bool _inicializado;

        public ProgresoDataRepository(
            ProgresoDefaultDataSourcePort defaultDataSource,
            ProgresoLocalDataSourcePort localDataSource)
        {
            _defaultDataSource = defaultDataSource;
            _localDataSource = localDataSource;
        }

        public ProgresoCapitulo ObtenerProgreso(string idCapitulo)
        {
            Inicializar();
            return _defaultDataSource.Leer(idCapitulo);
        }

        public void GuardarProgreso(ProgresoCapitulo progreso)
        {
            Inicializar();
            _defaultDataSource.Guardar(progreso);
            _localDataSource?.GuardarTodo(_defaultDataSource.LeerTodos());
        }

        public bool HayPartidaGuardada()
        {
            return _localDataSource != null && _localDataSource.Existe();
        }

        public void ReiniciarPartida()
        {
            _inicializado = true;        // no volver a cargar el archivo viejo
            _defaultDataSource.Limpiar();
            _localDataSource?.GuardarTodo(_defaultDataSource.LeerTodos());
        }

        private void Inicializar()
        {
            if (_inicializado) return;
            _inicializado = true;

            var guardados = _localDataSource?.LeerTodo();
            if (guardados == null) return;
            foreach (var progreso in guardados)
                _defaultDataSource.Guardar(progreso);
        }
    }
}
