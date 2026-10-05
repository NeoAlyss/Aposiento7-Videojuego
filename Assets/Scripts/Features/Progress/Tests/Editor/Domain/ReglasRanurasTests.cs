using System.Collections.Generic;
using NUnit.Framework;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Tests.Editor.Domain
{
    public class ReglasRanurasTests
    {
        [Test]
        public void EsNumeroValido_SoloAceptaDe1ALaCantidadDeRanuras()
        {
            Assert.IsFalse(ReglasRanuras.EsNumeroValido(0));
            Assert.IsTrue(ReglasRanuras.EsNumeroValido(1));
            Assert.IsTrue(ReglasRanuras.EsNumeroValido(ReglasRanuras.CANTIDAD));
            Assert.IsFalse(ReglasRanuras.EsNumeroValido(ReglasRanuras.CANTIDAD + 1));
        }

        [Test]
        public void CalcularCapituloActual_PartidaNueva_VaEnElCapitulo1()
        {
            Assert.AreEqual(1, ReglasRanuras.CalcularCapituloActual(new List<ProgresoCapitulo>()));
            Assert.AreEqual(1, ReglasRanuras.CalcularCapituloActual(null));
        }

        [Test]
        public void CalcularCapituloActual_EsElSiguienteAlUltimoCompletado()
        {
            var capitulos = new List<ProgresoCapitulo>
            {
                new ProgresoCapitulo("chapter_01", 3, 100f, true),
                new ProgresoCapitulo("chapter_02", 1, 50f, false)
            };
            Assert.AreEqual(2, ReglasRanuras.CalcularCapituloActual(capitulos));
        }

        [Test]
        public void CalcularCapituloActual_NoSePasaDelUltimoCapitulo()
        {
            var capitulos = new List<ProgresoCapitulo>();
            for (int i = 1; i <= ReglasRanuras.TOTAL_CAPITULOS; i++)
                capitulos.Add(new ProgresoCapitulo($"chapter_{i:00}", 0, 0f, true));
            Assert.AreEqual(ReglasRanuras.TOTAL_CAPITULOS, ReglasRanuras.CalcularCapituloActual(capitulos));
        }

        [Test]
        public void SumarSegundos_SumaElTiempoDeTodosLosCapitulos()
        {
            var capitulos = new List<ProgresoCapitulo>
            {
                new ProgresoCapitulo("chapter_01", 0, 100f, true),
                new ProgresoCapitulo("chapter_02", 0, 25.5f, false)
            };
            Assert.AreEqual(125.5f, ReglasRanuras.SumarSegundos(capitulos), 0.001f);
        }

        [Test]
        public void CalcularPorcentaje_PorAhoraEsElValorProvisional()
        {
            Assert.AreEqual(ReglasRanuras.PORCENTAJE_PROVISIONAL, ReglasRanuras.CalcularPorcentaje(new List<ProgresoCapitulo>()));
        }

        [Test]
        public void PrimeraLibre_DevuelveLaPrimeraRanuraSinPartida()
        {
            var ranuras = new List<ResumenRanura>
            {
                new ResumenRanura(1, true, 1, 10, 0f, 0L),
                ResumenRanura.Vacia(2),
                ResumenRanura.Vacia(3)
            };
            Assert.AreEqual(2, ReglasRanuras.PrimeraLibre(ranuras));
        }

        [Test]
        public void PrimeraLibre_ConTodasOcupadas_NoDevuelveNinguna()
        {
            var ranuras = new List<ResumenRanura>
            {
                new ResumenRanura(1, true, 1, 10, 0f, 0L),
                new ResumenRanura(2, true, 1, 10, 0f, 0L)
            };
            Assert.AreEqual(ReglasRanuras.SIN_RANURA, ReglasRanuras.PrimeraLibre(ranuras));
        }

        [Test]
        public void FormatearFecha_SinFecha_MuestraUnGuion()
        {
            Assert.AreEqual("—", ReglasRanuras.FormatearFecha(0L));
        }
    }
}
