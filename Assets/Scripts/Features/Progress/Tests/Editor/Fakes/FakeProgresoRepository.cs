using System.Collections.Generic;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Tests.Editor.Fakes
{
    public class FakeProgresoRepository : ProgresoRepositoryPort
    {
        public readonly Dictionary<string, ProgresoCapitulo> Almacen = new Dictionary<string, ProgresoCapitulo>();
        public int VecesGuardado;
        public bool PartidaGuardada;
        public int VecesReiniciado;

        public ProgresoCapitulo ObtenerProgreso(string idCapitulo)
        {
            return Almacen.TryGetValue(idCapitulo, out var p) ? p : ProgresoCapitulo.Nuevo(idCapitulo);
        }

        public void GuardarProgreso(ProgresoCapitulo progreso)
        {
            Almacen[progreso.IdCapitulo] = progreso;
            VecesGuardado++;
        }

        public bool HayPartidaGuardada() => PartidaGuardada;

        public void ReiniciarPartida()
        {
            Almacen.Clear();
            PartidaGuardada = true;
            VecesReiniciado++;
        }
    }
}
