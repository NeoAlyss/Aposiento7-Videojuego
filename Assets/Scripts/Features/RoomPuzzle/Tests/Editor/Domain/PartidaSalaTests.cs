using System.Collections.Generic;
using NUnit.Framework;
using UniversalPlatform.Features.RoomPuzzle.Data;
using UniversalPlatform.Features.RoomPuzzle.Domain;

namespace UniversalPlatform.Features.RoomPuzzle.Tests.Editor.Domain
{
    public class PartidaSalaTests
    {
        /// <summary>
        /// Sala de prueba de 3x2 (entrada E, puerta P, llave L, invitado I):
        ///   . L .
        ///   P I E
        /// </summary>
        private static DefinicionSala SalaChica()
        {
            return new DefinicionSala("prueba", "Prueba", 3, 2, new Celda(2, 0), new Celda(0, 0),
                new[] { new Celda(1, 1) },
                new[] { new InvitadoSala(new Celda(1, 0), 0, "¡Salud!") });
        }

        [Test]
        public void Empieza_EnLaEntrada_SinLlaves()
        {
            var partida = new PartidaSala(SalaChica());

            Assert.AreEqual(new Celda(2, 0), partida.Posicion);
            Assert.AreEqual(1, partida.Camino.Count);
            Assert.AreEqual(0, partida.LlavesRecogidas.Count);
            Assert.IsFalse(partida.PuertaAbierta);
            Assert.IsFalse(partida.Completada);
        }

        [Test]
        public void NoPuedeSalirDeLaGrilla()
        {
            var partida = new PartidaSala(SalaChica());

            Assert.AreEqual(ResultadoPaso.Bloqueado, partida.Mover(Direccion.Este));
            Assert.AreEqual(ResultadoPaso.Bloqueado, partida.Mover(Direccion.Sur));
            Assert.AreEqual(1, partida.Camino.Count);
        }

        [Test]
        public void NoPuedeAtravesarAUnInvitado()
        {
            var partida = new PartidaSala(SalaChica());

            Assert.AreEqual(ResultadoPaso.Bloqueado, partida.Mover(Direccion.Oeste));
            Assert.AreEqual(new Celda(2, 0), partida.Posicion);
        }

        [Test]
        public void VolverALaBaldosaAnterior_RecogeElTrazo()
        {
            var partida = new PartidaSala(SalaChica());
            partida.Mover(Direccion.Norte);

            Assert.AreEqual(ResultadoPaso.Retrocedido, partida.Mover(Direccion.Sur));
            Assert.AreEqual(new Celda(2, 0), partida.Posicion);
            Assert.AreEqual(1, partida.Camino.Count);
        }

        [Test]
        public void Retroceder_DevuelveLaLlaveTomadaEnEsaBaldosa()
        {
            var partida = new PartidaSala(SalaChica());
            partida.Mover(Direccion.Norte);
            partida.Mover(Direccion.Oeste);          // toma la llave en (1, 1)
            Assert.AreEqual(1, partida.LlavesRecogidas.Count);

            Assert.AreEqual(ResultadoPaso.Retrocedido, partida.Mover(Direccion.Este));
            Assert.AreEqual(0, partida.LlavesRecogidas.Count);
        }

        [Test]
        public void NoPuedeCruzarSuTrazo_SoloDevolverse()
        {
            // 3x2 libre: dar la vuelta y volver a pisar la entrada es cruzar el trazo, no devolverse.
            var sala = new DefinicionSala("prueba", "Prueba", 3, 2, new Celda(2, 0), new Celda(0, 1),
                new Celda[0], new InvitadoSala[0]);
            var partida = new PartidaSala(sala);
            partida.Mover(Direccion.Norte);           // (2, 1)
            partida.Mover(Direccion.Oeste);           // (1, 1)
            partida.Mover(Direccion.Sur);             // (1, 0)

            Assert.AreEqual(ResultadoPaso.Bloqueado, partida.Mover(Direccion.Este), "(2, 0) es trazo viejo.");
            Assert.AreEqual(ResultadoPaso.Retrocedido, partida.Mover(Direccion.Norte), "(1, 1) es la anterior.");
        }

        [Test]
        public void LosMueblesBloqueanTodasSusBaldosas()
        {
            // Sillón de 2 baldosas en la fila de arriba: (0, 1) y (1, 1).
            var sala = new DefinicionSala("prueba", "Prueba", 3, 2, new Celda(2, 0), new Celda(0, 0),
                new Celda[0], new InvitadoSala[0], muebles: new[] { new MuebleSala(TipoMueble.Sillon, new Celda(0, 1), 2) });
            var partida = new PartidaSala(sala);
            partida.Mover(Direccion.Norte);

            Assert.AreEqual(ResultadoPaso.Bloqueado, partida.Mover(Direccion.Oeste));
            Assert.IsTrue(sala.HayMuebleEn(new Celda(0, 1)));
            Assert.IsFalse(sala.HayMuebleEn(new Celda(2, 1)));
        }

        [Test]
        public void PisarUnaLlave_LaRecogeYAbreLaPuerta()
        {
            var partida = new PartidaSala(SalaChica());
            partida.Mover(Direccion.Norte);

            Assert.AreEqual(ResultadoPaso.LlaveRecogida, partida.Mover(Direccion.Oeste));
            Assert.AreEqual(1, partida.LlavesRecogidas.Count);
            Assert.IsTrue(partida.PuertaAbierta);
        }

        [Test]
        public void LlegarALaPuertaConTodasLasLlaves_CompletaLaSala()
        {
            var partida = new PartidaSala(SalaChica());
            partida.Mover(Direccion.Norte);
            partida.Mover(Direccion.Oeste);
            partida.Mover(Direccion.Oeste);

            Assert.AreEqual(ResultadoPaso.Completada, partida.Mover(Direccion.Sur));
            Assert.IsTrue(partida.Completada);
            Assert.AreEqual(ResultadoPaso.Bloqueado, partida.Mover(Direccion.Norte), "Ya completada, no se mueve más.");
        }

        [Test]
        public void LlegarALaPuertaSinLlaves_NoCompleta()
        {
            // Misma sala sin invitado, para poder ir directo a la puerta por abajo.
            var sala = new DefinicionSala("prueba", "Prueba", 3, 2, new Celda(2, 0), new Celda(0, 0),
                new[] { new Celda(1, 1) }, new InvitadoSala[0]);
            var partida = new PartidaSala(sala);
            partida.Mover(Direccion.Oeste);

            Assert.AreEqual(ResultadoPaso.PuertaCerrada, partida.Mover(Direccion.Oeste));
            Assert.IsFalse(partida.Completada);
        }

        [Test]
        public void Deshacer_RetrocedeYDevuelveLaLlave()
        {
            var partida = new PartidaSala(SalaChica());
            partida.Mover(Direccion.Norte);
            partida.Mover(Direccion.Oeste);

            Assert.IsTrue(partida.Deshacer());
            Assert.AreEqual(new Celda(2, 1), partida.Posicion);
            Assert.AreEqual(0, partida.LlavesRecogidas.Count);
            Assert.AreEqual(ResultadoPaso.LlaveRecogida, partida.Mover(Direccion.Oeste), "La baldosa vuelve a estar libre.");
        }

        [Test]
        public void Deshacer_EnLaEntrada_NoHaceNada()
        {
            Assert.IsFalse(new PartidaSala(SalaChica()).Deshacer());
        }

        [Test]
        public void Atascada_CuandoNoQuedaNingunPaso()
        {
            // Pasillo de 3x1 con la entrada al centro: tras ir al este no queda adónde ir.
            var sala = new DefinicionSala("prueba", "Prueba", 3, 1, new Celda(1, 0), new Celda(0, 0),
                new[] { new Celda(2, 0) }, new InvitadoSala[0]);
            var partida = new PartidaSala(sala);
            Assert.IsFalse(partida.Atascada);

            partida.Mover(Direccion.Este);

            Assert.IsTrue(partida.Atascada, "Al este hay pared y la entrada ya está pisada.");
        }

        [Test]
        public void InvitadosCerca_SoloLosDeBaldosasVecinas()
        {
            var partida = new PartidaSala(SalaChica());

            Assert.AreEqual(1, partida.InvitadosCerca().Count, "El invitado está junto a la entrada.");

            partida.Mover(Direccion.Norte);
            Assert.AreEqual(0, partida.InvitadosCerca().Count, "En diagonal no cuenta.");
        }

        [Test]
        public void Capitulo1_EsLaSalaNegra_ConTresLlaves_YSePuedeResolver()
        {
            var sala = new SalasDataRepository().Obtener("chapter_01");

            Assert.IsNotNull(sala);
            Assert.AreEqual("Aposento negro", sala.Nombre);
            Assert.AreEqual(3, sala.TotalLlaves);
            Assert.AreEqual(20, sala.Ancho);
            Assert.AreEqual(5, sala.Alto);
            ComprobarSala(sala);
        }

        [Test]
        public void AposentoAzul_SigueGuardado_YSePuedeResolver()
        {
            var sala = new SalasDataRepository().Obtener(SalasDataRepository.ID_SALA_AZUL);

            Assert.IsNotNull(sala);
            Assert.AreEqual(3, sala.TotalLlaves);
            ComprobarSala(sala);
        }

        private static void ComprobarSala(DefinicionSala sala)
        {
            Assert.IsFalse(sala.EstaOcupada(sala.Entrada), "La entrada debe estar libre.");
            Assert.IsFalse(sala.EstaOcupada(sala.Salida), "La salida debe estar libre.");
            foreach (var invitado in sala.Invitados)
            {
                Assert.IsTrue(sala.EstaDentro(invitado.Celda));
                Assert.IsFalse(sala.HayLlaveEn(invitado.Celda));
                Assert.IsFalse(sala.HayMuebleEn(invitado.Celda), $"Invitado sobre un mueble en {invitado.Celda}.");
            }
            foreach (var mueble in sala.Muebles)
                for (int dx = 0; dx < mueble.Ancho; dx++)
                {
                    var c = new Celda(mueble.Celda.X + dx, mueble.Celda.Y);
                    Assert.IsTrue(sala.EstaDentro(c), $"Mueble fuera de la sala en {c}.");
                    Assert.IsFalse(sala.HayLlaveEn(c), $"Mueble sobre una llave en {c}.");
                }
            Assert.IsTrue(TieneSolucion(new PartidaSala(sala)), $"{sala.Nombre} debe poder cruzarse.");
        }

        /// <summary>
        /// Búsqueda en profundidad con las mismas reglas del juego (sin usar el retroceso, que solo
        /// deshace). Poda las ramas en que alguna llave o la salida ya quedaron inalcanzables.
        /// </summary>
        private static bool TieneSolucion(PartidaSala partida)
        {
            if (!SigueAlcanzable(partida)) return false;
            foreach (var direccion in new[] { Direccion.Oeste, Direccion.Norte, Direccion.Sur, Direccion.Este })
            {
                var destino = partida.Posicion.Hacia(direccion);
                if (!partida.PuedePisar(destino)) continue;
                var resultado = partida.MoverA(destino);
                if (resultado == ResultadoPaso.Completada) return true;
                if (TieneSolucion(partida)) return true;
                partida.Deshacer();
            }
            return false;
        }

        private static bool SigueAlcanzable(PartidaSala partida)
        {
            var sala = partida.Sala;
            var visto = new HashSet<Celda> { partida.Posicion };
            var pila = new Stack<Celda>();
            pila.Push(partida.Posicion);
            while (pila.Count > 0)
            {
                var c = pila.Pop();
                foreach (var d in new[] { Direccion.Norte, Direccion.Sur, Direccion.Este, Direccion.Oeste })
                {
                    var n = c.Hacia(d);
                    if (visto.Contains(n) || !partida.PuedePisar(n)) continue;
                    visto.Add(n);
                    if (n != sala.Salida) pila.Push(n);
                }
            }
            if (!visto.Contains(sala.Salida)) return false;
            foreach (var llave in sala.Llaves)
            {
                bool tomada = false;
                foreach (var l in partida.LlavesRecogidas) if (l == llave) { tomada = true; break; }
                if (!tomada && !visto.Contains(llave)) return false;
            }
            return true;
        }
    }
}
