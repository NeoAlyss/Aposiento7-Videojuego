using System;

namespace UniversalPlatform.Features.ChapterSelect.Domain
{
    /// <summary>
    /// Matemática del reloj (sin dependencias de Unity): navegación entre números y ángulos de las
    /// manillas. Los ángulos son horarios desde las 12 y ACUMULADOS (no se reinician a 0-360), así
    /// las animaciones nunca dan saltos ni la vuelta larga.
    /// </summary>
    public static class ReglasReloj
    {
        public const int CANTIDAD_NUMEROS = 12;
        public const float GRADOS_POR_HORA = 30f;
        public const int VUELTAS_MINUTERO_POR_HORA = 12;

        public static bool EsNumeroValido(int numero)
        {
            return numero >= 1 && numero <= CANTIDAD_NUMEROS;
        }

        /// <summary>Avanza (+1, sentido del reloj) o retrocede (-1) con vuelta 12 -> 1. Sin selección: +1 da 1, -1 da 12.</summary>
        public static int MoverNumero(int actual, int delta)
        {
            if (!EsNumeroValido(actual)) return delta > 0 ? 1 : CANTIDAD_NUMEROS;
            int indice = ((actual - 1 + delta) % CANTIDAD_NUMEROS + CANTIDAD_NUMEROS) % CANTIDAD_NUMEROS;
            return indice + 1;
        }

        public static float AnguloHoraria(int numero) => numero * GRADOS_POR_HORA;

        /// <summary>Diferencia más corta entre dos ángulos, en el rango [-180, 180].</summary>
        public static float DeltaAngulo(float desde, float hasta)
        {
            float d = (hasta - desde) % 360f;
            if (d > 180f) d -= 360f;
            else if (d < -180f) d += 360f;
            return d;
        }

        /// <summary>Ángulo acumulado de la horaria para apuntar al número por el camino más corto.</summary>
        public static float AcumularAnguloHoraria(float anguloAcumulado, int numero)
        {
            return anguloAcumulado + DeltaAngulo(anguloAcumulado, AnguloHoraria(numero));
        }

        /// <summary>Como en un reloj real: 12 vueltas de minutero por cada vuelta de la horaria.</summary>
        public static float AnguloMinutero(float anguloHorariaAcumulado)
        {
            return anguloHorariaAcumulado * VUELTAS_MINUTERO_POR_HORA;
        }

        /// <summary>Cuántas horas recorre un giro (sirve para alargar la animación en saltos grandes).</summary>
        public static float PasosRecorridos(float deltaGrados)
        {
            return Math.Abs(deltaGrados) / GRADOS_POR_HORA;
        }

        /// <summary>Los números 1 a 6 están en la mitad derecha del reloj; 7 a 12 en la izquierda.</summary>
        public static bool EsLadoDerecho(int numero)
        {
            return numero >= 1 && numero <= 6;
        }

        /// <summary>Posición (x a la derecha, y hacia arriba) del número sobre un círculo de radio dado.</summary>
        public static (float X, float Y) PosicionNumero(int numero, float radio)
        {
            double angulo = AnguloHoraria(numero) * Math.PI / 180.0;
            return ((float)(Math.Sin(angulo) * radio), (float)(Math.Cos(angulo) * radio));
        }
    }
}
