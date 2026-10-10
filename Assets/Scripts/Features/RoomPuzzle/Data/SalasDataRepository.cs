using System;
using System.Collections.Generic;
using UniversalPlatform.Features.RoomPuzzle.Domain;

namespace UniversalPlatform.Features.RoomPuzzle.Data
{
    /// <summary>
    /// Catálogo de aposentos, escrito en código. Cada sala se dibuja como un mapa de texto (ver
    /// <see cref="DesdeMapa"/>), así que se puede rediseñar moviendo letras. Para agregar una sala,
    /// añade su definición con el id del capítulo (chapter_02, chapter_03...).
    /// </summary>
    public class SalasDataRepository : SalasRepositoryPort
    {
        /// <summary>El aposento azul queda guardado, pero hoy ningún capítulo lo usa.</summary>
        public const string ID_SALA_AZUL = "aposento_azul";

        // Aspecto de un invitado = máscara + 3 * color. Así lo espera el constructor de la escena,
        // que carga los sprites Characters/Invitados/invitado_NN.png en ese orden.
        public const int GATO = 0, ZORRO = 1, HALCON = 2;
        public const int ORIGINAL = 0, PLATA = 1, CARMESI = 2, JADE = 3, VIOLETA = 4;
        public static int Aspecto(int mascara, int color) => mascara + 3 * color;

        private readonly Dictionary<string, DefinicionSala> _salas = new Dictionary<string, DefinicionSala>();

        public SalasDataRepository()
        {
            Agregar(AposentoNegro());
            Agregar(AposentoAzul());
        }

        public DefinicionSala Obtener(string idCapitulo)
        {
            return idCapitulo != null && _salas.TryGetValue(idCapitulo, out var sala) ? sala : null;
        }

        private void Agregar(DefinicionSala sala) => _salas[sala.IdCapitulo] = sala;

        /// <summary>
        /// Capítulo 1 · Aposento negro: terciopelo negro y vitrales escarlata. Aquí está el reloj de
        /// ébano, y de él sale la máscara (el encapuchado). Pocos invitados se atreven a entrar.
        /// </summary>
        private static DefinicionSala AposentoNegro()
        {
            return DesdeMapa("chapter_01", "Aposento negro", new[]
                {
                    // fila 4 (fondo) ... fila 0 (frente). E = reloj (entrada), X = puerta (salida).
                    "..BB.....PL.C.BB.PL.",
                    "..I..SS.I.....I.....",
                    "XI....I..I..I.SS...E",
                    "...I..M...I...P.I...",
                    "L...P.P.....M.......",
                },
                new[]
                {
                    // Invitados en orden de lectura del mapa (de arriba abajo, de izquierda a derecha).
                    Invitado(GATO, PLATA, "Nadie quiere quedarse en esta sala...", "¿Oyes el reloj? Cada vez suena más fuerte."),
                    Invitado(ZORRO, CARMESI, "Esa luz roja me pone la piel de gallina.", "Bailemos en otra sala, ¿quieres?"),
                    Invitado(HALCON, ORIGINAL, "El príncipe dice que aquí nada puede tocarnos.", "¡Brindo por el príncipe Próspero!"),
                    Invitado(HALCON, JADE, "Cuando dé la hora, todos callaremos otra vez.", "¿Cuántas campanadas faltan?"),
                    Invitado(GATO, ORIGINAL, "¡Que no pare la música!", "¡Otra copa! Esta noche invita el príncipe."),
                    Invitado(ZORRO, VIOLETA, "Los vitrales parecen sangrar.", "Afuera, dicen, ya no queda nadie."),
                    Invitado(GATO, CARMESI, "¡Mil invitados y ni una sola pena!", "¡A bailar, que la noche es joven!"),
                    Invitado(ZORRO, ORIGINAL, "Aquí dentro estamos a salvo.", "Los muros son altos y las puertas, de hierro."),
                    Invitado(HALCON, PLATA, "¿Quién trajo ese disfraz tan rojo?", "Qué máscara tan extraña la de aquel..."),
                    Invitado(GATO, JADE, "Me pareció ver algo salir del reloj.", "No, no... habrá sido el vino."),
                });
        }

        /// <summary>
        /// Aposento azul (guardado, sin capítulo por ahora): la antesala, el nacimiento.
        /// </summary>
        private static DefinicionSala AposentoAzul()
        {
            return DesdeMapa(ID_SALA_AZUL, "Aposento azul", new[]
                {
                    "..L...",
                    "....I.",
                    "X..ILE",
                    "......",
                    "L.I...",
                },
                new[]
                {
                    // (4, 3) zorro, (3, 2) gato, (2, 0) halcón: en orden de lectura.
                    Invitado(ZORRO, ORIGINAL, "Aquí dentro nada puede alcanzarnos.", "¡Mira esos vitrales! Parece que amanece."),
                    Invitado(GATO, ORIGINAL, "¡Que no pare la música!", "¡Otra copa! Esta noche invita el príncipe."),
                    Invitado(HALCON, ORIGINAL, "¡Mil invitados y ni una sola pena!", "¡A bailar, que la noche es joven!"),
                });
        }

        private sealed class DatosInvitado
        {
            public int Aspecto;
            public string[] Frases;
        }

        private static DatosInvitado Invitado(int mascara, int color, params string[] frases) =>
            new DatosInvitado { Aspecto = Aspecto(mascara, color), Frases = frases };

        /// <summary>
        /// Arma una sala a partir de un mapa de texto. La primera línea es el fondo de la sala (fila de
        /// más arriba) y la última, el frente; cada carácter es una baldosa:
        /// <list type="bullet">
        /// <item><c>.</c> libre · <c>E</c> entrada · <c>X</c> salida (frente a la puerta) · <c>L</c> llave</item>
        /// <item><c>I</c> invitado (toma el siguiente de la lista, en orden de lectura)</item>
        /// <item><c>B</c> librería · <c>S</c> sillón (dos letras seguidas = un mueble de 2 baldosas)</item>
        /// <item><c>P</c> planta · <c>M</c> mesita · <c>C</c> candelabro</item>
        /// </list>
        /// </summary>
        private static DefinicionSala DesdeMapa(string id, string nombre, string[] mapa, DatosInvitado[] invitados)
        {
            int alto = mapa.Length;
            int ancho = mapa[0].Length;
            Celda? entrada = null, salida = null;
            var llaves = new List<Celda>();
            var listaInvitados = new List<InvitadoSala>();
            var muebles = new List<MuebleSala>();
            int siguienteInvitado = 0;

            for (int fila = 0; fila < alto; fila++)
            {
                if (mapa[fila].Length != ancho) throw new ArgumentException($"Sala {id}: la fila {fila} no mide {ancho}.");
                int y = alto - 1 - fila;
                for (int x = 0; x < ancho; x++)
                {
                    char c = mapa[fila][x];
                    var celda = new Celda(x, y);
                    switch (c)
                    {
                        case '.': break;
                        case 'E': entrada = celda; break;
                        case 'X': salida = celda; break;
                        case 'L': llaves.Add(celda); break;
                        case 'I':
                            if (siguienteInvitado >= invitados.Length)
                                throw new ArgumentException($"Sala {id}: hay más invitados en el mapa que en la lista.");
                            var datos = invitados[siguienteInvitado++];
                            listaInvitados.Add(new InvitadoSala(celda, datos.Aspecto, datos.Frases));
                            break;
                        case 'B':
                        case 'S':
                            // Muebles anchos: hasta 2 letras iguales seguidas forman una sola pieza.
                            int largo = 1;
                            if (x + 1 < ancho && mapa[fila][x + 1] == c) largo = 2;
                            muebles.Add(new MuebleSala(c == 'B' ? TipoMueble.Libreria : TipoMueble.Sillon, celda, largo));
                            x += largo - 1;
                            break;
                        case 'P': muebles.Add(new MuebleSala(TipoMueble.Planta, celda)); break;
                        case 'M': muebles.Add(new MuebleSala(TipoMueble.Mesita, celda)); break;
                        case 'C': muebles.Add(new MuebleSala(TipoMueble.Candelabro, celda)); break;
                        default: throw new ArgumentException($"Sala {id}: carácter desconocido '{c}' en {celda}.");
                    }
                }
            }
            if (entrada == null || salida == null) throw new ArgumentException($"Sala {id}: falta la entrada (E) o la salida (X).");

            return new DefinicionSala(id, nombre, ancho, alto, entrada.Value, salida.Value, llaves, listaInvitados,
                muebles: muebles);
            // Sin textos de la narradora: la cinemática hace de introducción y el final de la demo
            // lo resuelve la vista. Si otra sala los necesita, se pasan en textosDeEntrada/Salida.
        }
    }
}
