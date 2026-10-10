using System.Collections.Generic;
using UniversalPlatform.Features.ChapterSelect.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Data
{
    public class CapitulosDataRepository : CapitulosRepositoryPort
    {
        private readonly CapitulosDefaultDataSourcePort _defaultDataSource;

        public CapitulosDataRepository(CapitulosDefaultDataSourcePort defaultDataSource)
        {
            _defaultDataSource = defaultDataSource;
        }

        public IReadOnlyList<Capitulo> ObtenerCapitulos()
        {
            return _defaultDataSource.LeerCapitulos();
        }

        public Capitulo ObtenerPorNumero(int numeroReloj)
        {
            foreach (var capitulo in _defaultDataSource.LeerCapitulos())
                if (capitulo.NumeroReloj == numeroReloj) return capitulo;
            return null;
        }
    }
}
