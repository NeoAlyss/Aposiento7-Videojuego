using System;

namespace UniversalPlatform.Features.MainMenu.Domain
{
    public static class ReglasMenuPrincipal
    {
        public const int CANTIDAD_OPCIONES = 4;
        public const int SIN_SELECCION = -1;

        public static bool EsIndiceValido(int indice)
        {
            return indice >= 0 && indice < CANTIDAD_OPCIONES;
        }

        public static OpcionMenuPrincipal ObtenerOpcion(int indice)
        {
            if (!EsIndiceValido(indice)) throw new ArgumentOutOfRangeException(nameof(indice));
            return (OpcionMenuPrincipal)indice;
        }

        /// <summary>
        /// Mueve la selección con vuelta al inicio/final. Sin selección previa, avanzar (+1) elige la
        /// primera opción y retroceder (-1) elige la última.
        /// </summary>
        public static int MoverSeleccion(int indiceActual, int delta)
        {
            if (!EsIndiceValido(indiceActual))
                return delta > 0 ? 0 : CANTIDAD_OPCIONES - 1;
            return ((indiceActual + delta) % CANTIDAD_OPCIONES + CANTIDAD_OPCIONES) % CANTIDAD_OPCIONES;
        }

        public static string ObtenerNombre(OpcionMenuPrincipal opcion)
        {
            return opcion switch
            {
                OpcionMenuPrincipal.NuevaPartida => "Nueva partida",
                OpcionMenuPrincipal.CargarPartida => "Cargar partida",
                OpcionMenuPrincipal.Opciones => "Opciones",
                OpcionMenuPrincipal.Salir => "Salir",
                _ => "Desconocida"
            };
        }
    }
}
