using System.Collections.Generic;
using UniversalPlatform.Features.Progress.Data;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Datasources
{
    /// <summary>Ranuras de guardado sobre archivos JSON: reutiliza el lector/escritor de una partida.</summary>
    public class JsonFileRanurasLocalDataSource : RanurasLocalDataSourcePort
    {
        private readonly string _directorioBase;

        public JsonFileRanurasLocalDataSource(string directorioBase)
        {
            _directorioBase = directorioBase;
        }

        private JsonFileProgresoLocalDataSource Archivo(int numero) =>
            new JsonFileProgresoLocalDataSource(_directorioBase, ArchivosRanura.Nombre(numero));

        public bool Existe(int numero) => Archivo(numero).Existe();

        public IReadOnlyList<ProgresoCapitulo> LeerCapitulos(int numero) => Archivo(numero).LeerTodo();

        public long LeerGuardadoUnixMs(int numero) => Archivo(numero).LeerGuardadoUnixMs();

        public void CrearVacia(int numero) => Archivo(numero).GuardarTodo(new List<ProgresoCapitulo>());

        public void Borrar(int numero) => Archivo(numero).Borrar();
    }
}
