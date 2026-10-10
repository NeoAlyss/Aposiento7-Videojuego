using System;
using System.Collections.Generic;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Data
{
    public class RanurasDataRepository : RanurasRepositoryPort
    {
        private readonly RanurasLocalDataSourcePort _localDataSource;

        public RanurasDataRepository(RanurasLocalDataSourcePort localDataSource)
        {
            _localDataSource = localDataSource ?? throw new ArgumentNullException(nameof(localDataSource));
        }

        public IReadOnlyList<ResumenRanura> ObtenerResumenes()
        {
            var lista = new List<ResumenRanura>(ReglasRanuras.CANTIDAD);
            for (int n = 1; n <= ReglasRanuras.CANTIDAD; n++)
            {
                if (!_localDataSource.Existe(n))
                {
                    lista.Add(ResumenRanura.Vacia(n));
                    continue;
                }

                // Un archivo dañado cuenta como partida recién creada: la vela se ve encendida y se
                // puede reemplazar, en vez de desaparecer.
                IReadOnlyList<ProgresoCapitulo> capitulos = _localDataSource.LeerCapitulos(n) ?? new List<ProgresoCapitulo>();
                lista.Add(new ResumenRanura(
                    n,
                    true,
                    ReglasRanuras.CalcularCapituloActual(capitulos),
                    ReglasRanuras.CalcularPorcentaje(capitulos),
                    ReglasRanuras.SumarSegundos(capitulos),
                    _localDataSource.LeerGuardadoUnixMs(n)));
            }
            return lista;
        }

        public void SeleccionarRanura(int numero)
        {
            if (!ReglasRanuras.EsNumeroValido(numero)) return;
            RanuraActiva.Numero = numero;
        }

        public void IniciarPartidaEn(int numero)
        {
            if (!ReglasRanuras.EsNumeroValido(numero)) return;
            _localDataSource.CrearVacia(numero);
            RanuraActiva.Numero = numero;
        }

        public void BorrarPartida(int numero)
        {
            if (!ReglasRanuras.EsNumeroValido(numero)) return;
            _localDataSource.Borrar(numero);
        }
    }
}
