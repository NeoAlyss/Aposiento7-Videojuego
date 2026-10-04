using System.Collections.Generic;
using UniversalPlatform.Features.ChapterSelect.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Data
{
    /// <summary>
    /// Los 12 capítulos del reloj. MVP: solo el capítulo 1 está disponible; los demás aparecen
    /// bloqueados. Para habilitar otro más adelante basta con poner disponible = true.
    /// </summary>
    public class CapitulosDefaultDataSource : CapitulosDefaultDataSourcePort
    {
        private readonly IReadOnlyList<Capitulo> _capitulos = Crear();

        public IReadOnlyList<Capitulo> LeerCapitulos() => _capitulos;

        private static IReadOnlyList<Capitulo> Crear()
        {
            var lista = new List<Capitulo>();
            for (int n = 1; n <= ReglasReloj.CANTIDAD_NUMEROS; n++)
            {
                lista.Add(new Capitulo(
                    id: $"chapter_{n:00}",
                    numeroReloj: n,
                    titulo: $"Capítulo {n}",
                    dificultad: n <= 4 ? DificultadCapitulo.Facil : n <= 8 ? DificultadCapitulo.Media : DificultadCapitulo.Dificil,
                    totalLlaves: 3,
                    disponible: n == 1,
                    nombreEscena: $"Chapter{n}"));
            }
            return lista;
        }
    }
}
