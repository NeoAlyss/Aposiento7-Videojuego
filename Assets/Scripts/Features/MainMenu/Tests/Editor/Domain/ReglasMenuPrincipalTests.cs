using System;
using NUnit.Framework;
using UniversalPlatform.Features.MainMenu.Domain;

namespace UniversalPlatform.Features.MainMenu.Tests.Editor.Domain
{
    public class ReglasMenuPrincipalTests
    {
        [TestCase(-1, true, 0)]        // sin selección + avanzar -> primera
        [TestCase(-1, false, 3)]       // sin selección + retroceder -> última
        [TestCase(0, true, 1)]
        [TestCase(3, true, 0)]         // vuelta al inicio
        [TestCase(0, false, 3)]        // vuelta al final
        [TestCase(2, false, 1)]
        public void MoverSeleccion_HaceVueltaCircular(int actual, bool avanzar, int esperado)
        {
            Assert.AreEqual(esperado, ReglasMenuPrincipal.MoverSeleccion(actual, avanzar ? 1 : -1));
        }

        [TestCase(-1, false)]
        [TestCase(0, true)]
        [TestCase(3, true)]
        [TestCase(4, false)]
        public void EsIndiceValido(int indice, bool esperado)
        {
            Assert.AreEqual(esperado, ReglasMenuPrincipal.EsIndiceValido(indice));
        }

        [Test]
        public void ObtenerOpcion_RespetaElOrdenDelMenu()
        {
            Assert.AreEqual(OpcionMenuPrincipal.NuevaPartida, ReglasMenuPrincipal.ObtenerOpcion(0));
            Assert.AreEqual(OpcionMenuPrincipal.CargarPartida, ReglasMenuPrincipal.ObtenerOpcion(1));
            Assert.AreEqual(OpcionMenuPrincipal.Opciones, ReglasMenuPrincipal.ObtenerOpcion(2));
            Assert.AreEqual(OpcionMenuPrincipal.Salir, ReglasMenuPrincipal.ObtenerOpcion(3));
        }

        [Test]
        public void ObtenerOpcion_ConIndiceInvalido_Lanza()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ReglasMenuPrincipal.ObtenerOpcion(7));
        }

        [Test]
        public void ObtenerNombre_DevuelveTextosEnEspanol()
        {
            Assert.AreEqual("Nueva partida", ReglasMenuPrincipal.ObtenerNombre(OpcionMenuPrincipal.NuevaPartida));
            Assert.AreEqual("Salir", ReglasMenuPrincipal.ObtenerNombre(OpcionMenuPrincipal.Salir));
        }
    }
}
