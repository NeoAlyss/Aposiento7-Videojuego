using System.Collections.Generic;
using NUnit.Framework;
using UniversalPlatform.Features.RoomPuzzle.Domain;
using UniversalPlatform.Features.RoomPuzzle.Presentation.Room;

namespace UniversalPlatform.Features.RoomPuzzle.Tests.Editor.Presentation
{
    public class RoomPuzzleViewModelTests
    {
        /// <summary>
        ///   . L .
        ///   P I E      con un texto de entrada y uno de salida
        /// </summary>
        private static DefinicionSala Sala(bool conTextos = true)
        {
            return new DefinicionSala("prueba", "Prueba", 3, 2, new Celda(2, 0), new Celda(0, 0),
                new[] { new Celda(1, 1) },
                new[] { new InvitadoSala(new Celda(1, 0), 0, "¡Salud!", "¡Otra ronda!") },
                conTextos ? new[] { "Entro." } : new string[0],
                conTextos ? new[] { "Salgo." } : new string[0]);
        }

        private RoomPuzzleViewModel _vm;
        private List<ResultadoPaso> _pasos;
        private int _llavesAlCompletar;
        private int _salidas;

        private void Crear(bool conTextos = true)
        {
            _vm = new RoomPuzzleViewModel(Sala(conTextos));
            _pasos = new List<ResultadoPaso>();
            _llavesAlCompletar = -1;
            _salidas = 0;
            _vm.OnPaso += r => _pasos.Add(r);
            _vm.OnSalaCompletada += llaves => _llavesAlCompletar = llaves;
            _vm.OnSalir += () => _salidas++;
            _vm.Inicializar();
        }

        private void Resolver()
        {
            _vm.Mover(Direccion.Norte);
            _vm.Mover(Direccion.Oeste);
            _vm.Mover(Direccion.Oeste);
            _vm.Mover(Direccion.Sur);
        }

        [Test]
        public void EmpiezaConElTextoDeEntrada_YNoDejaMoverse()
        {
            Crear();

            Assert.AreEqual(FaseSala.Entrada, _vm.EstadoActual.Fase);
            Assert.AreEqual("Entro.", _vm.EstadoActual.TextoNarradora);

            _vm.Mover(Direccion.Norte);
            Assert.AreEqual(1, _vm.EstadoActual.Camino.Count);
        }

        [Test]
        public void Continuar_TrasElUltimoTexto_EmpiezaElPuzle()
        {
            Crear();

            _vm.Continuar();

            Assert.AreEqual(FaseSala.Jugando, _vm.EstadoActual.Fase);
            Assert.AreEqual(string.Empty, _vm.EstadoActual.TextoNarradora);
        }

        [Test]
        public void SinTextosDeEntrada_EmpiezaJugando()
        {
            Crear(conTextos: false);

            Assert.AreEqual(FaseSala.Jugando, _vm.EstadoActual.Fase);
        }

        [Test]
        public void JuntoAUnInvitado_EsteDiceUnaFrase_YAlAlejarseCalla()
        {
            Crear(conTextos: false);

            Assert.AreEqual("¡Salud!", _vm.EstadoActual.FrasesDeInvitados[0]);

            _vm.Mover(Direccion.Norte);
            Assert.IsNull(_vm.EstadoActual.FrasesDeInvitados[0]);
        }

        [Test]
        public void AlVolverAAcercarse_ElInvitadoCambiaDeFrase()
        {
            Crear(conTextos: false);
            _vm.Mover(Direccion.Norte);      // se aleja (queda en diagonal)

            _vm.Mover(Direccion.Oeste);      // encima del invitado: vuelve a estar al lado

            Assert.AreEqual("¡Otra ronda!", _vm.EstadoActual.FrasesDeInvitados[0]);
        }

        [Test]
        public void Resolver_AvisaConLasLlaves_YPasaALosTextosDeSalida()
        {
            Crear();
            _vm.Continuar();

            Resolver();

            Assert.AreEqual(ResultadoPaso.Completada, _pasos[_pasos.Count - 1]);
            Assert.AreEqual(1, _llavesAlCompletar);
            Assert.AreEqual(FaseSala.Salida, _vm.EstadoActual.Fase);
            Assert.AreEqual("Salgo.", _vm.EstadoActual.TextoNarradora);
            Assert.AreEqual(0, _salidas);

            _vm.Continuar();
            Assert.AreEqual(1, _salidas);
        }

        [Test]
        public void Resolver_SinTextosDeSalida_SaleDeInmediato()
        {
            Crear(conTextos: false);

            Resolver();

            Assert.AreEqual(1, _salidas);
        }

        [Test]
        public void Deshacer_Y_Reiniciar_VuelvenAtras()
        {
            Crear(conTextos: false);
            _vm.Mover(Direccion.Norte);
            _vm.Mover(Direccion.Oeste);
            Assert.AreEqual(1, _vm.EstadoActual.Llaves);

            _vm.Deshacer();
            Assert.AreEqual(0, _vm.EstadoActual.Llaves);
            Assert.AreEqual(new Celda(2, 1), _vm.EstadoActual.Posicion);

            _vm.Reiniciar();
            Assert.AreEqual(new Celda(2, 0), _vm.EstadoActual.Posicion);
            Assert.AreEqual(1, _vm.EstadoActual.Camino.Count);
        }

        [Test]
        public void MoverA_UnaBaldosaLejana_NoHaceNada()
        {
            Crear(conTextos: false);

            _vm.MoverA(new Celda(0, 1));

            Assert.AreEqual(ResultadoPaso.Bloqueado, _pasos[0]);
            Assert.AreEqual(new Celda(2, 0), _vm.EstadoActual.Posicion);
        }
    }
}
