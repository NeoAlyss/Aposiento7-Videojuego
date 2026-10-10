using NUnit.Framework;
using UniversalPlatform.Features.ChapterSelect.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Tests.Editor.Domain
{
    public class ReglasRelojTests
    {
        [TestCase(1, 1, 2)]
        [TestCase(12, 1, 1)]           // vuelta 12 -> 1
        [TestCase(1, -1, 12)]          // vuelta 1 -> 12
        [TestCase(7, -1, 6)]
        [TestCase(0, 1, 1)]            // sin selección: avanzar da 1
        [TestCase(0, -1, 12)]          // sin selección: retroceder da 12
        public void MoverNumero(int actual, int delta, int esperado)
        {
            Assert.AreEqual(esperado, ReglasReloj.MoverNumero(actual, delta));
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(12, true)]
        [TestCase(13, false)]
        public void EsNumeroValido(int numero, bool esperado)
        {
            Assert.AreEqual(esperado, ReglasReloj.EsNumeroValido(numero));
        }

        [TestCase(0f, 30f, 30f)]
        [TestCase(0f, 180f, 180f)]
        [TestCase(30f, 330f, -60f)]    // el camino corto pasa por las 12
        [TestCase(350f, 10f, 20f)]
        [TestCase(10f, 350f, -20f)]
        public void DeltaAngulo_EligeElCaminoMasCorto(float desde, float hasta, float esperado)
        {
            Assert.AreEqual(esperado, ReglasReloj.DeltaAngulo(desde, hasta), 0.001f);
        }

        [Test]
        public void AcumularAnguloHoraria_DelInicioAlSeis_DaMediaVuelta()
        {
            Assert.AreEqual(180f, ReglasReloj.AcumularAnguloHoraria(0f, 6), 0.001f);
        }

        [Test]
        public void AcumularAnguloHoraria_DelOnceAlUno_NoDaLaVueltaLarga()
        {
            // Horaria en las 11 (330°) hacia la 1: debe avanzar +60° hasta 390°, no retroceder 300°.
            Assert.AreEqual(390f, ReglasReloj.AcumularAnguloHoraria(330f, 1), 0.001f);
        }

        [Test]
        public void AcumularAnguloHoraria_UnoADoceDeUnaHora_RetrocedeUnPaso()
        {
            Assert.AreEqual(0f, ReglasReloj.AcumularAnguloHoraria(30f, 12), 0.001f);
        }

        [Test]
        public void AnguloMinutero_DaDoceVueltasPorVueltaDeHoraria()
        {
            Assert.AreEqual(360f, ReglasReloj.AnguloMinutero(30f), 0.001f);     // una hora = una vuelta
            Assert.AreEqual(2160f, ReglasReloj.AnguloMinutero(180f), 0.001f);   // seis horas = seis vueltas
        }

        [Test]
        public void PasosRecorridos_CuentaHoras()
        {
            Assert.AreEqual(1f, ReglasReloj.PasosRecorridos(30f), 0.001f);
            Assert.AreEqual(2f, ReglasReloj.PasosRecorridos(-60f), 0.001f);
        }

        [TestCase(1, true)]
        [TestCase(6, true)]
        [TestCase(7, false)]
        [TestCase(12, false)]
        public void EsLadoDerecho(int numero, bool esperado)
        {
            Assert.AreEqual(esperado, ReglasReloj.EsLadoDerecho(numero));
        }

        [Test]
        public void PosicionNumero_UbicaLosCuatroPuntosCardinales()
        {
            AssertPos(ReglasReloj.PosicionNumero(12, 100f), 0f, 100f);
            AssertPos(ReglasReloj.PosicionNumero(3, 100f), 100f, 0f);
            AssertPos(ReglasReloj.PosicionNumero(6, 100f), 0f, -100f);
            AssertPos(ReglasReloj.PosicionNumero(9, 100f), -100f, 0f);
        }

        private static void AssertPos((float X, float Y) pos, float x, float y)
        {
            Assert.AreEqual(x, pos.X, 0.01f);
            Assert.AreEqual(y, pos.Y, 0.01f);
        }
    }
}
