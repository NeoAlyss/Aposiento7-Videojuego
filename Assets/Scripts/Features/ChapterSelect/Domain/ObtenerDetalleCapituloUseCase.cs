using System;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Domain
{
    /// <summary>Junta el capítulo con su progreso y devuelve los textos de los boxes de información.</summary>
    public class ObtenerDetalleCapituloUseCase
    {
        private readonly CapitulosRepositoryPort _capitulos;
        private readonly ObtenerProgresoCapituloUseCase _obtenerProgreso;

        public ObtenerDetalleCapituloUseCase(
            CapitulosRepositoryPort capitulos,
            ObtenerProgresoCapituloUseCase obtenerProgreso)
        {
            _capitulos = capitulos ?? throw new ArgumentNullException(nameof(capitulos));
            _obtenerProgreso = obtenerProgreso ?? throw new ArgumentNullException(nameof(obtenerProgreso));
        }

        public DetalleCapitulo Ejecutar(int numeroReloj)
        {
            var capitulo = _capitulos.ObtenerPorNumero(numeroReloj);

            if (capitulo == null)
                return new DetalleCapitulo(numeroReloj, ReglasCapitulo.TituloSinCapitulo(numeroReloj), "—", "—", "—", true);

            if (!capitulo.Disponible)
                return new DetalleCapitulo(numeroReloj, capitulo.Titulo, "Bloqueado", "—", "—", true);

            var progreso = _obtenerProgreso.Ejecutar(capitulo.Id);   // nunca jugado: 0 llaves y 00:00:00
            return new DetalleCapitulo(
                numeroReloj,
                capitulo.Titulo,
                ReglasCapitulo.NombreDificultad(capitulo.Dificultad),
                ReglasProgreso.FormatearLlaves(progreso.Llaves, capitulo.TotalLlaves),
                ReglasProgreso.FormatearTiempo(progreso.SegundosJugados),
                false);
        }
    }
}
