using NUnit.Framework;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Tests.Editor.Domain
{
    public class ReglasProgresoTests
    {
        [TestCase(0f, "00:00:00")]
        [TestCase(59.9f, "00:00:59")]
        [TestCase(60f, "00:01:00")]
        [TestCase(3661.9f, "01:01:01")]
        [TestCase(-5f, "00:00:00")]
        [TestCase(float.NaN, "00:00:00")]
        public void FormatearTiempo_DevuelveHHMMSS(float segundos, string esperado)
        {
            Assert.AreEqual(esperado, ReglasProgreso.FormatearTiempo(segundos));
        }

        [Test]
        public void FormatearLlaves_MuestraConseguidasSobreTotal()
        {
            Assert.AreEqual("2 / 3", ReglasProgreso.FormatearLlaves(2, 3));
            Assert.AreEqual("0 / 0", ReglasProgreso.FormatearLlaves(-1, -4));
        }

        [Test]
        public void SumarTiempo_IgnoraDeltasInvalidos()
        {
            Assert.AreEqual(10f, ReglasProgreso.SumarTiempo(10f, 0f));
            Assert.AreEqual(10f, ReglasProgreso.SumarTiempo(10f, -3f));
            Assert.AreEqual(10f, ReglasProgreso.SumarTiempo(10f, float.NaN));
            Assert.AreEqual(12.5f, ReglasProgreso.SumarTiempo(10f, 2.5f));
        }

        [Test]
        public void SumarLlaves_IgnoraCantidadesNoPositivas()
        {
            Assert.AreEqual(1, ReglasProgreso.SumarLlaves(1, 0));
            Assert.AreEqual(1, ReglasProgreso.SumarLlaves(1, -2));
            Assert.AreEqual(3, ReglasProgreso.SumarLlaves(1, 2));
        }

        [Test]
        public void ProgresoNuevo_ParteEnCero()
        {
            var p = ProgresoCapitulo.Nuevo("chapter_01");
            Assert.AreEqual(0, p.Llaves);
            Assert.AreEqual(0f, p.SegundosJugados);
            Assert.IsFalse(p.Completado);
        }
    }
}
