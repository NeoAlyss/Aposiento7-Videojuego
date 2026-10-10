using System;
using System.Collections.Generic;

namespace UniversalPlatform.Features.RoomPuzzle.Domain
{
    /// <summary>Un invitado de la fiesta: ocupa una baldosa (no se puede pisar) y habla solo, sin ver a nadie.</summary>
    public sealed class InvitadoSala
    {
        public Celda Celda { get; }
        /// <summary>Qué dibujo usa (0, 1, 2...). La vista elige el sprite.</summary>
        public int Aspecto { get; }
        /// <summary>Frases de fiesta. Nunca van dirigidas al protagonista: los invitados no lo ven.</summary>
        public IReadOnlyList<string> Frases { get; }

        public InvitadoSala(Celda celda, int aspecto, params string[] frases)
        {
            Celda = celda;
            Aspecto = aspecto;
            Frases = frases ?? new string[0];
        }
    }

    /// <summary>Muebles y adornos que ocupan baldosas. La vista elige el dibujo de cada tipo.</summary>
    public enum TipoMueble
    {
        /// <summary>Librería contra el fondo; suele ocupar 2 baldosas de ancho.</summary>
        Libreria,
        /// <summary>Sillón; suele ocupar 2 baldosas de ancho.</summary>
        Sillon,
        Planta,
        Mesita,
        Candelabro
    }

    /// <summary>Un mueble sobre el suelo: bloquea sus baldosas como un invitado, pero no habla.</summary>
    public sealed class MuebleSala
    {
        public TipoMueble Tipo { get; }
        /// <summary>Baldosa de más a la izquierda que ocupa.</summary>
        public Celda Celda { get; }
        /// <summary>Cuántas baldosas ocupa hacia el este (1 o más).</summary>
        public int Ancho { get; }

        public MuebleSala(TipoMueble tipo, Celda celda, int ancho = 1)
        {
            Tipo = tipo;
            Celda = celda;
            Ancho = Math.Max(1, ancho);
        }

        public bool Ocupa(Celda c) => c.Y == Celda.Y && c.X >= Celda.X && c.X < Celda.X + Ancho;
    }

    /// <summary>
    /// Un aposento: la grilla de baldosas, por dónde se entra y se sale, dónde están las llaves y los
    /// invitados, y los textos de la narradora. Es solo datos: las reglas viven en <see cref="PartidaSala"/>.
    /// </summary>
    public sealed class DefinicionSala
    {
        public string IdCapitulo { get; }
        public string Nombre { get; }
        public int Ancho { get; }
        public int Alto { get; }
        /// <summary>Baldosa donde aparece el protagonista (lado este).</summary>
        public Celda Entrada { get; }
        /// <summary>Baldosa frente a la puerta (lado oeste). Llegar aquí con todas las llaves completa la sala.</summary>
        public Celda Salida { get; }
        public IReadOnlyList<Celda> Llaves { get; }
        public IReadOnlyList<InvitadoSala> Invitados { get; }
        public IReadOnlyList<MuebleSala> Muebles { get; }
        public IReadOnlyList<string> TextosDeEntrada { get; }
        public IReadOnlyList<string> TextosDeSalida { get; }

        public int TotalLlaves => Llaves.Count;

        public DefinicionSala(string idCapitulo, string nombre, int ancho, int alto, Celda entrada, Celda salida,
            IReadOnlyList<Celda> llaves, IReadOnlyList<InvitadoSala> invitados,
            IReadOnlyList<string> textosDeEntrada = null, IReadOnlyList<string> textosDeSalida = null,
            IReadOnlyList<MuebleSala> muebles = null)
        {
            if (ancho < 1 || alto < 1) throw new ArgumentException("La sala necesita al menos una baldosa.");
            IdCapitulo = idCapitulo;
            Nombre = nombre;
            Ancho = ancho;
            Alto = alto;
            Entrada = entrada;
            Salida = salida;
            Llaves = llaves ?? new List<Celda>();
            Invitados = invitados ?? new List<InvitadoSala>();
            Muebles = muebles ?? new List<MuebleSala>();
            TextosDeEntrada = textosDeEntrada ?? new List<string>();
            TextosDeSalida = textosDeSalida ?? new List<string>();
        }

        public bool EstaDentro(Celda celda) => celda.X >= 0 && celda.X < Ancho && celda.Y >= 0 && celda.Y < Alto;

        public bool HayInvitadoEn(Celda celda)
        {
            foreach (var invitado in Invitados)
                if (invitado.Celda == celda) return true;
            return false;
        }

        public bool HayMuebleEn(Celda celda)
        {
            foreach (var mueble in Muebles)
                if (mueble.Ocupa(celda)) return true;
            return false;
        }

        /// <summary>Invitado o mueble: esa baldosa no se puede pisar.</summary>
        public bool EstaOcupada(Celda celda) => HayInvitadoEn(celda) || HayMuebleEn(celda);

        public bool HayLlaveEn(Celda celda)
        {
            foreach (var llave in Llaves)
                if (llave == celda) return true;
            return false;
        }
    }
}
