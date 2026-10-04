using System.Collections.Generic;
using UniversalPlatform.Features.ChapterSelect.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Presentation.Clock
{
    public class ChapterSelectViewState
    {
        public int NumeroSeleccionado { get; }
        public DetalleCapitulo Detalle { get; }

        /// <summary>Ángulos ACUMULADOS (grados horarios desde las 12) hacia los que deben girar las manillas.</summary>
        public float AnguloHorariaObjetivo { get; }
        public float AnguloMinuteroObjetivo { get; }

        /// <summary>Horas recorridas en este movimiento (para alargar la animación en saltos grandes).</summary>
        public float PasosRecorridos { get; }

        public bool NumeroEnLadoDerecho { get; }

        /// <summary>Índice = número del reloj - 1. True si ese capítulo está bloqueado o no existe.</summary>
        public IReadOnlyList<bool> BloqueadosPorNumero { get; }

        public ChapterSelectViewState(
            int numeroSeleccionado,
            DetalleCapitulo detalle,
            float anguloHorariaObjetivo,
            float anguloMinuteroObjetivo,
            float pasosRecorridos,
            bool numeroEnLadoDerecho,
            IReadOnlyList<bool> bloqueadosPorNumero)
        {
            NumeroSeleccionado = numeroSeleccionado;
            Detalle = detalle;
            AnguloHorariaObjetivo = anguloHorariaObjetivo;
            AnguloMinuteroObjetivo = anguloMinuteroObjetivo;
            PasosRecorridos = pasosRecorridos;
            NumeroEnLadoDerecho = numeroEnLadoDerecho;
            BloqueadosPorNumero = bloqueadosPorNumero;
        }
    }
}
