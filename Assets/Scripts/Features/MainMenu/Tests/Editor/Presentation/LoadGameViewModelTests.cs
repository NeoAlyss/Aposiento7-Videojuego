using System.Collections.Generic;
using NUnit.Framework;
using UniversalPlatform.Features.MainMenu.Presentation.LoadGame;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.MainMenu.Tests.Editor.Presentation
{
    public class LoadGameViewModelTests
    {
        /// <summary>Ranuras en memoria: guarda cuáles tienen partida y cuál quedó activa.</summary>
        private class RanurasFalsas : RanurasRepositoryPort
        {
            public readonly HashSet<int> ConPartida = new HashSet<int>();
            public int Activa;
            public int PartidasIniciadas;

            public IReadOnlyList<ResumenRanura> ObtenerResumenes()
            {
                var lista = new List<ResumenRanura>();
                for (int n = 1; n <= ReglasRanuras.CANTIDAD; n++)
                    lista.Add(ConPartida.Contains(n) ? new ResumenRanura(n, true, 1, 10, 65f, 0L) : ResumenRanura.Vacia(n));
                return lista;
            }

            public void SeleccionarRanura(int numero) { Activa = numero; }

            public void IniciarPartidaEn(int numero)
            {
                ConPartida.Add(numero);
                Activa = numero;
                PartidasIniciadas++;
            }
        }

        private RanurasFalsas _repo;
        private LoadGameViewModel _vm;
        private int _entradas;
        private List<int> _vacias;

        [SetUp]
        public void SetUp()
        {
            _repo = new RanurasFalsas();
            _vm = new LoadGameViewModel(
                new ObtenerRanurasUseCase(_repo),
                new CargarRanuraUseCase(_repo),
                new IniciarPartidaEnRanuraUseCase(_repo));
            _entradas = 0;
            _vacias = new List<int>();
            _vm.OnEntrarAPartida += () => _entradas++;
            _vm.OnRanuraVacia += i => _vacias.Add(i);
            _vm.Inicializar();
        }

        [Test]
        public void Inicializar_EmpiezaOculto()
        {
            Assert.IsFalse(_vm.EstadoActual.Visible);
            Assert.AreEqual(ReglasRanuras.CANTIDAD, _vm.EstadoActual.Ranuras.Count);
        }

        [Test]
        public void NuevaPartida_ConRanuraLibre_LaUsaYEntraDirecto()
        {
            _repo.ConPartida.Add(1);

            _vm.SolicitarNuevaPartida();

            Assert.AreEqual(1, _entradas);
            Assert.AreEqual(2, _repo.Activa, "Debe usar la primera vela libre.");
            Assert.IsFalse(_vm.EstadoActual.Visible, "No hace falta mostrar las velas.");
        }

        [Test]
        public void NuevaPartida_ConTodasOcupadas_AbreLasVelasParaElegirCualReemplazar()
        {
            for (int n = 1; n <= ReglasRanuras.CANTIDAD; n++) _repo.ConPartida.Add(n);

            _vm.SolicitarNuevaPartida();

            Assert.AreEqual(0, _entradas);
            Assert.IsTrue(_vm.EstadoActual.Visible);
            Assert.AreEqual(ModoPartidas.Reemplazar, _vm.EstadoActual.Modo);

            _vm.Activar(2);     // tercera vela

            Assert.AreEqual(1, _entradas);
            Assert.AreEqual(3, _repo.Activa);
            Assert.AreEqual(1, _repo.PartidasIniciadas);
        }

        [Test]
        public void AbrirCarga_SeleccionaLaPrimeraVelaEncendida()
        {
            _repo.ConPartida.Add(2);

            _vm.AbrirCarga();

            Assert.IsTrue(_vm.EstadoActual.Visible);
            Assert.AreEqual(ModoPartidas.Cargar, _vm.EstadoActual.Modo);
            Assert.AreEqual(1, _vm.EstadoActual.IndiceSeleccionado);
            StringAssert.Contains("Capítulo 1", _vm.EstadoActual.TextoInfo);
            StringAssert.Contains("10%", _vm.EstadoActual.TextoInfo);
            StringAssert.Contains("00:01:05", _vm.EstadoActual.TextoInfo);
        }

        [Test]
        public void AbrirCarga_SinPartidas_NoSeleccionaNada()
        {
            _vm.AbrirCarga();

            Assert.AreEqual(LoadGameViewState.SIN_SELECCION, _vm.EstadoActual.IndiceSeleccionado);
            Assert.AreEqual(string.Empty, _vm.EstadoActual.TextoInfo);
        }

        [Test]
        public void Cargar_VelaEncendida_LaDejaActivaYEntra()
        {
            _repo.ConPartida.Add(3);
            _vm.AbrirCarga();

            _vm.Activar(2);

            Assert.AreEqual(1, _entradas);
            Assert.AreEqual(3, _repo.Activa);
            Assert.AreEqual(0, _repo.PartidasIniciadas, "Cargar no debe borrar la partida.");
        }

        [Test]
        public void Cargar_VelaApagada_NoEntraYAvisa()
        {
            _vm.AbrirCarga();

            _vm.Activar(0);

            Assert.AreEqual(0, _entradas);
            Assert.AreEqual(1, _vacias.Count);
            Assert.AreEqual(0, _vacias[0]);
            Assert.IsTrue(_vm.EstadoActual.Visible);
        }

        [Test]
        public void Mover_RecorreLasVelasYLuegoElBotonVolver()
        {
            _vm.AbrirCarga();

            for (int i = 0; i < ReglasRanuras.CANTIDAD; i++) _vm.Mover(+1);
            Assert.AreEqual(ReglasRanuras.CANTIDAD - 1, _vm.EstadoActual.IndiceSeleccionado);

            _vm.Mover(+1);
            Assert.IsTrue(_vm.EstadoActual.VolverSeleccionado);

            _vm.Mover(+1);
            Assert.AreEqual(0, _vm.EstadoActual.IndiceSeleccionado, "Da la vuelta a la primera vela.");
        }

        [Test]
        public void ConfirmarSobreVolver_CierraElSubmenu()
        {
            _vm.AbrirCarga();
            _vm.Mover(-1);      // sin selección, retroceder elige "Volver"

            _vm.Confirmar();

            Assert.IsFalse(_vm.EstadoActual.Visible);
            Assert.AreEqual(0, _entradas);
        }

        [Test]
        public void Oculto_IgnoraElInput()
        {
            _vm.Mover(+1);
            _vm.Activar(0);
            _vm.Confirmar();

            Assert.AreEqual(LoadGameViewState.SIN_SELECCION, _vm.EstadoActual.IndiceSeleccionado);
            Assert.AreEqual(0, _entradas);
        }

        [Test]
        public void DespuesDeEntrar_IgnoraMasClics()
        {
            _repo.ConPartida.Add(1);
            _repo.ConPartida.Add(2);
            _vm.AbrirCarga();

            _vm.Activar(0);
            _vm.Activar(1);

            Assert.AreEqual(1, _entradas);
            Assert.AreEqual(1, _repo.Activa);
        }
    }
}
