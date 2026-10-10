using System.Collections.Generic;
using UniversalPlatform.Features.RoomPuzzle.Domain;

namespace UniversalPlatform.Features.RoomPuzzle.Presentation.Room
{
    public enum FaseSala
    {
        /// <summary>Pantalla negra con los textos de la narradora antes de ver la sala.</summary>
        Entrada,
        Jugando,
        /// <summary>Sala resuelta: textos de cierre antes de volver al reloj.</summary>
        Salida
    }

    /// <summary>Foto del aposento para la vista. Inmutable: las listas son copias.</summary>
    public class RoomPuzzleViewState
    {
        public DefinicionSala Sala { get; }
        public FaseSala Fase { get; }
        /// <summary>Texto de la narradora que toca mostrar en las fases de entrada y salida.</summary>
        public string TextoNarradora { get; }
        public IReadOnlyList<Celda> Camino { get; }
        public Celda Posicion { get; }
        public IReadOnlyList<Celda> LlavesRecogidas { get; }
        public bool PuertaAbierta { get; }
        public bool Atascada { get; }
        /// <summary>Está en la baldosa de la puerta sin todas las llaves.</summary>
        public bool PuertaNoCede { get; }
        /// <summary>Por invitado (mismo orden que en la sala): la frase que dice ahora, o null si calla.</summary>
        public IReadOnlyList<string> FrasesDeInvitados { get; }

        public int Llaves => LlavesRecogidas.Count;
        public int TotalLlaves => Sala.TotalLlaves;

        public RoomPuzzleViewState(DefinicionSala sala, FaseSala fase, string textoNarradora,
            IReadOnlyList<Celda> camino, IReadOnlyList<Celda> llavesRecogidas, bool puertaAbierta,
            bool atascada, bool puertaNoCede, IReadOnlyList<string> frasesDeInvitados)
        {
            Sala = sala;
            Fase = fase;
            TextoNarradora = textoNarradora ?? string.Empty;
            Camino = camino;
            Posicion = camino[camino.Count - 1];
            LlavesRecogidas = llavesRecogidas;
            PuertaAbierta = puertaAbierta;
            Atascada = atascada;
            PuertaNoCede = puertaNoCede;
            FrasesDeInvitados = frasesDeInvitados;
        }
    }
}
