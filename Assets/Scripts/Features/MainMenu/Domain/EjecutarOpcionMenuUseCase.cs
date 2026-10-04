using System;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.MainMenu.Domain
{
    public class EjecutarOpcionMenuUseCase
    {
        private readonly IniciarNuevaPartidaUseCase _iniciarNuevaPartida;
        private readonly HayPartidaGuardadaUseCase _hayPartidaGuardada;

        public EjecutarOpcionMenuUseCase(
            IniciarNuevaPartidaUseCase iniciarNuevaPartida,
            HayPartidaGuardadaUseCase hayPartidaGuardada)
        {
            _iniciarNuevaPartida = iniciarNuevaPartida ?? throw new ArgumentNullException(nameof(iniciarNuevaPartida));
            _hayPartidaGuardada = hayPartidaGuardada ?? throw new ArgumentNullException(nameof(hayPartidaGuardada));
        }

        public ResultadoAccionMenu Ejecutar(OpcionMenuPrincipal opcion)
        {
            switch (opcion)
            {
                case OpcionMenuPrincipal.NuevaPartida:
                    _iniciarNuevaPartida.Ejecutar();
                    return ResultadoAccionMenu.IrASeleccionCapitulo;

                case OpcionMenuPrincipal.CargarPartida:
                    return _hayPartidaGuardada.Ejecutar()
                        ? ResultadoAccionMenu.IrASeleccionCapitulo
                        : ResultadoAccionMenu.PartidaNoEncontrada;

                case OpcionMenuPrincipal.Opciones:
                    return ResultadoAccionMenu.MostrarOpciones;

                case OpcionMenuPrincipal.Salir:
                    return ResultadoAccionMenu.SalirDelJuego;

                default:
                    throw new ArgumentOutOfRangeException(nameof(opcion), opcion, null);
            }
        }
    }
}
