using System;
using System.Collections.Generic;
using UniversalPlatform.Features.RoomPuzzle.Domain;

namespace UniversalPlatform.Features.RoomPuzzle.Presentation.Room
{
    /// <summary>
    /// Lleva un aposento de principio a fin: textos de entrada, el puzle del suelo y textos de salida.
    /// No conoce Unity ni el guardado: avisa con eventos y el Installer decide qué persistir.
    /// </summary>
    public class RoomPuzzleViewModel
    {
        private readonly PartidaSala _partida;
        private readonly int[] _turnoDeFrase;        // qué frase le toca a cada invitado
        private readonly bool[] _estabaCerca;

        private FaseSala _fase;
        private int _indiceTexto;

        public RoomPuzzleViewState EstadoActual { get; private set; }

        public event Action<RoomPuzzleViewState> OnStateChanged;
        /// <summary>Resultado de cada intento de paso (para sonido y animación).</summary>
        public event Action<ResultadoPaso> OnPaso;
        /// <summary>La sala se resolvió: llegan las llaves conseguidas. Es el momento de guardar.</summary>
        public event Action<int> OnSalaCompletada;
        /// <summary>Terminaron los textos de salida: toca volver al reloj.</summary>
        public event Action OnSalir;

        public RoomPuzzleViewModel(DefinicionSala sala)
        {
            if (sala == null) throw new ArgumentNullException(nameof(sala));
            _partida = new PartidaSala(sala);
            _turnoDeFrase = new int[sala.Invitados.Count];
            _estabaCerca = new bool[sala.Invitados.Count];
        }

        public void Inicializar()
        {
            _partida.Reiniciar();
            _indiceTexto = 0;
            _fase = _partida.Sala.TextosDeEntrada.Count > 0 ? FaseSala.Entrada : FaseSala.Jugando;
            Publicar();
        }

        /// <summary>Espacio / Enter / clic durante los textos: pasa al siguiente o termina la fase.</summary>
        public void Continuar()
        {
            if (_fase == FaseSala.Entrada)
            {
                _indiceTexto++;
                if (_indiceTexto >= _partida.Sala.TextosDeEntrada.Count) _fase = FaseSala.Jugando;
                Publicar();
            }
            else if (_fase == FaseSala.Salida)
            {
                _indiceTexto++;
                if (_indiceTexto >= _partida.Sala.TextosDeSalida.Count)
                {
                    OnSalir?.Invoke();
                    return;
                }
                Publicar();
            }
        }

        public void Mover(Direccion direccion) => Paso(_partida.Posicion.Hacia(direccion));

        /// <summary>Clic en una baldosa: solo avanza si es vecina de la posición actual.</summary>
        public void MoverA(Celda destino) => Paso(destino);

        public void Deshacer()
        {
            if (_fase != FaseSala.Jugando) return;
            if (_partida.Deshacer()) Publicar();
        }

        public void Reiniciar()
        {
            if (_fase != FaseSala.Jugando) return;
            _partida.Reiniciar();
            Publicar();
        }

        private void Paso(Celda destino)
        {
            if (_fase != FaseSala.Jugando) return;

            var resultado = _partida.MoverA(destino);
            if (resultado == ResultadoPaso.Completada)
            {
                _fase = FaseSala.Salida;
                _indiceTexto = 0;
            }
            if (resultado != ResultadoPaso.Bloqueado) Publicar();
            OnPaso?.Invoke(resultado);

            if (resultado == ResultadoPaso.Completada)
            {
                OnSalaCompletada?.Invoke(_partida.LlavesRecogidas.Count);
                if (_partida.Sala.TextosDeSalida.Count == 0) OnSalir?.Invoke();
            }
        }

        private void Publicar()
        {
            var sala = _partida.Sala;

            // Cada invitado dice una frase mientras el protagonista está al lado; al volver a
            // acercarse, pasa a la siguiente.
            var frases = new string[sala.Invitados.Count];
            if (_fase == FaseSala.Jugando)
            {
                var cerca = new HashSet<int>(_partida.InvitadosCerca());
                for (int i = 0; i < sala.Invitados.Count; i++)
                {
                    bool ahora = cerca.Contains(i);
                    var opciones = sala.Invitados[i].Frases;
                    if (ahora && opciones.Count > 0)
                    {
                        if (!_estabaCerca[i] && EstadoActual != null) _turnoDeFrase[i]++;
                        frases[i] = opciones[_turnoDeFrase[i] % opciones.Count];
                    }
                    _estabaCerca[i] = ahora;
                }
            }

            string narradora = string.Empty;
            if (_fase == FaseSala.Entrada && _indiceTexto < sala.TextosDeEntrada.Count)
                narradora = sala.TextosDeEntrada[_indiceTexto];
            else if (_fase == FaseSala.Salida && _indiceTexto < sala.TextosDeSalida.Count)
                narradora = sala.TextosDeSalida[_indiceTexto];

            EstadoActual = new RoomPuzzleViewState(
                sala,
                _fase,
                narradora,
                new List<Celda>(_partida.Camino),
                new List<Celda>(_partida.LlavesRecogidas),
                _partida.PuertaAbierta,
                _fase == FaseSala.Jugando && _partida.Atascada,
                _fase == FaseSala.Jugando && _partida.Posicion == sala.Salida && !_partida.PuertaAbierta,
                frases);
            OnStateChanged?.Invoke(EstadoActual);
        }
    }
}
