using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UniversalPlatform.Features.Progress.Data;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Datasources
{
    public class JsonFileProgresoLocalDataSource : ProgresoLocalDataSourcePort
    {
        private readonly string _rutaArchivo;

        /// <param name="nombreArchivo">Archivo de la ranura; por defecto, el de la ranura 1.</param>
        public JsonFileProgresoLocalDataSource(string directorioBase, string nombreArchivo = ArchivosRanura.NOMBRE_RANURA_1)
        {
            _rutaArchivo = Path.Combine(directorioBase, nombreArchivo);
        }

        /// <summary>Elimina el archivo de la partida (si existe).</summary>
        public void Borrar()
        {
            try
            {
                if (File.Exists(_rutaArchivo)) File.Delete(_rutaArchivo);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonFileProgresoLocalDataSource] Error borrando la partida: {ex.Message}");
            }
        }

        /// <summary>Momento del último guardado en milisegundos Unix; 0 si no hay archivo o está dañado.</summary>
        public long LeerGuardadoUnixMs()
        {
            try
            {
                if (!File.Exists(_rutaArchivo)) return 0L;
                var dto = JsonUtility.FromJson<ProgresoPartidaDto>(File.ReadAllText(_rutaArchivo));
                return dto == null ? 0L : dto.timestampModificacion;
            }
            catch (Exception)
            {
                return 0L;
            }
        }

        public bool Existe() => File.Exists(_rutaArchivo);

        public IReadOnlyList<ProgresoCapitulo> LeerTodo()
        {
            try
            {
                if (!File.Exists(_rutaArchivo)) return null;
                var json = File.ReadAllText(_rutaArchivo);
                var dto = JsonUtility.FromJson<ProgresoPartidaDto>(json);
                if (dto == null || dto.capitulos == null) return null;

                var lista = new List<ProgresoCapitulo>();
                foreach (var c in dto.capitulos)
                {
                    if (string.IsNullOrEmpty(c.idCapitulo)) continue;
                    lista.Add(new ProgresoCapitulo(c.idCapitulo, c.llaves, c.segundosJugados, c.completado));
                }
                return lista;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void GuardarTodo(IReadOnlyCollection<ProgresoCapitulo> progresos)
        {
            try
            {
                var dto = new ProgresoPartidaDto
                {
                    timestampModificacion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                foreach (var p in progresos)
                {
                    dto.capitulos.Add(new ProgresoCapituloDto
                    {
                        idCapitulo = p.IdCapitulo,
                        llaves = p.Llaves,
                        segundosJugados = p.SegundosJugados,
                        completado = p.Completado
                    });
                }

                var directorio = Path.GetDirectoryName(_rutaArchivo);
                if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);
                File.WriteAllText(_rutaArchivo, JsonUtility.ToJson(dto, true));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonFileProgresoLocalDataSource] Error escribiendo progreso: {ex.Message}");
            }
        }
    }
}
