using System;

namespace UniversalPlatform.Features.RoomPuzzle.Domain
{
    /// <summary>Una baldosa del suelo. X crece hacia el este (derecha); Y, hacia el fondo de la sala (arriba).</summary>
    public struct Celda : IEquatable<Celda>
    {
        public int X { get; }
        public int Y { get; }

        public Celda(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Celda Hacia(Direccion direccion)
        {
            switch (direccion)
            {
                case Direccion.Norte: return new Celda(X, Y + 1);
                case Direccion.Sur: return new Celda(X, Y - 1);
                case Direccion.Este: return new Celda(X + 1, Y);
                case Direccion.Oeste: return new Celda(X - 1, Y);
                default: return this;
            }
        }

        /// <summary>Vecinas en cruz (no en diagonal).</summary>
        public bool EsVecinaDe(Celda otra) => Math.Abs(X - otra.X) + Math.Abs(Y - otra.Y) == 1;

        public bool Equals(Celda otra) => X == otra.X && Y == otra.Y;
        public override bool Equals(object obj) => obj is Celda otra && Equals(otra);
        public override int GetHashCode() => X * 397 ^ Y;
        public static bool operator ==(Celda a, Celda b) => a.Equals(b);
        public static bool operator !=(Celda a, Celda b) => !a.Equals(b);
        public override string ToString() => $"({X}, {Y})";
    }

    public enum Direccion
    {
        /// <summary>Hacia el fondo de la sala (arriba en pantalla).</summary>
        Norte,
        Sur,
        /// <summary>Hacia la entrada (derecha en pantalla).</summary>
        Este,
        /// <summary>Hacia la puerta de salida (izquierda en pantalla).</summary>
        Oeste
    }
}
