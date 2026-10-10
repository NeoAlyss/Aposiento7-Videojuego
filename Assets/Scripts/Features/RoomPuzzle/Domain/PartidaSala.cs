using System.Collections.Generic;

namespace UniversalPlatform.Features.RoomPuzzle.Domain
{
    /// <summary>Qué pasó al intentar dar un paso.</summary>
    public enum ResultadoPaso
    {
        /// <summary>Avanzó a una baldosa libre.</summary>
        Movido,
        /// <summary>Avanzó y recogió una llave.</summary>
        LlaveRecogida,
        /// <summary>Avanzó a la baldosa de la puerta, pero faltan llaves: la puerta no cede.</summary>
        PuertaCerrada,
        /// <summary>Llegó a la puerta con todas las llaves: sala completada.</summary>
        Completada,
        /// <summary>No se movió: hay una pared, un invitado, un mueble o una baldosa ya pisada.</summary>
        Bloqueado,
        /// <summary>Volvió a la baldosa de la que venía: el trazo se recoge un paso (y la llave, si la tomó ahí, vuelve).</summary>
        Retrocedido
    }

    /// <summary>
    /// Un intento de cruzar el aposento. El protagonista avanza de baldosa en baldosa dejando un
    /// trazo; no puede salir de la grilla, atravesar invitados o muebles ni cruzar su propio trazo,
    /// pero sí devolverse por él: pisar la baldosa de la que viene recoge el trazo un paso.
    /// Gana al llegar a la baldosa de la puerta con todas las llaves. Sin Unity.
    /// </summary>
    public sealed class PartidaSala
    {
        private readonly List<Celda> _camino = new List<Celda>();
        private readonly HashSet<Celda> _pisadas = new HashSet<Celda>();
        private readonly List<Celda> _llaves = new List<Celda>();

        public DefinicionSala Sala { get; }
        /// <summary>Baldosas recorridas en orden; la primera es la entrada y la última, la posición actual.</summary>
        public IReadOnlyList<Celda> Camino => _camino;
        /// <summary>Llaves recogidas en este intento, en el orden en que se tomaron.</summary>
        public IReadOnlyList<Celda> LlavesRecogidas => _llaves;
        public Celda Posicion => _camino[_camino.Count - 1];
        public bool PuertaAbierta => _llaves.Count >= Sala.TotalLlaves;
        public bool Completada { get; private set; }

        public PartidaSala(DefinicionSala sala)
        {
            Sala = sala;
            Reiniciar();
        }

        public void Reiniciar()
        {
            _camino.Clear();
            _pisadas.Clear();
            _llaves.Clear();
            Completada = false;
            _camino.Add(Sala.Entrada);
            _pisadas.Add(Sala.Entrada);
        }

        public bool PuedePisar(Celda celda) =>
            Sala.EstaDentro(celda) && !Sala.EstaOcupada(celda) && !_pisadas.Contains(celda);

        /// <summary>Baldosa de la que viene (la penúltima del trazo); no existe en la entrada.</summary>
        private bool EsLaAnterior(Celda celda) => _camino.Count >= 2 && _camino[_camino.Count - 2] == celda;

        public ResultadoPaso Mover(Direccion direccion) => MoverA(Posicion.Hacia(direccion));

        /// <summary>Paso a una baldosa concreta; solo vale si es vecina de la posición actual.</summary>
        public ResultadoPaso MoverA(Celda destino)
        {
            if (Completada || !destino.EsVecinaDe(Posicion)) return ResultadoPaso.Bloqueado;

            // Devolverse por el propio trazo: se recoge el último paso.
            if (EsLaAnterior(destino))
            {
                Deshacer();
                return ResultadoPaso.Retrocedido;
            }

            if (!PuedePisar(destino)) return ResultadoPaso.Bloqueado;

            _camino.Add(destino);
            _pisadas.Add(destino);

            bool tomoLlave = Sala.HayLlaveEn(destino);
            if (tomoLlave) _llaves.Add(destino);

            if (destino == Sala.Salida)
            {
                if (!PuertaAbierta) return ResultadoPaso.PuertaCerrada;
                Completada = true;
                return ResultadoPaso.Completada;
            }
            return tomoLlave ? ResultadoPaso.LlaveRecogida : ResultadoPaso.Movido;
        }

        /// <summary>Retrocede un paso (devuelve la llave si la había tomado ahí). false si está en la entrada.</summary>
        public bool Deshacer()
        {
            if (Completada || _camino.Count <= 1) return false;
            var ultima = Posicion;
            _camino.RemoveAt(_camino.Count - 1);
            _pisadas.Remove(ultima);
            _llaves.Remove(ultima);
            return true;
        }

        /// <summary>No queda ningún paso posible y la sala no está completada: toca deshacer o reiniciar.</summary>
        public bool Atascada
        {
            get
            {
                if (Completada) return false;
                // Siempre se puede retroceder, así que "atascada" es no tener ninguna baldosa nueva.
                var p = Posicion;
                return !PuedePisar(p.Hacia(Direccion.Norte)) && !PuedePisar(p.Hacia(Direccion.Sur))
                    && !PuedePisar(p.Hacia(Direccion.Este)) && !PuedePisar(p.Hacia(Direccion.Oeste));
            }
        }

        /// <summary>Índices (en <see cref="DefinicionSala.Invitados"/>) de los invitados en una baldosa vecina.</summary>
        public IReadOnlyList<int> InvitadosCerca()
        {
            var cerca = new List<int>();
            for (int i = 0; i < Sala.Invitados.Count; i++)
                if (Sala.Invitados[i].Celda.EsVecinaDe(Posicion)) cerca.Add(i);
            return cerca;
        }
    }
}
