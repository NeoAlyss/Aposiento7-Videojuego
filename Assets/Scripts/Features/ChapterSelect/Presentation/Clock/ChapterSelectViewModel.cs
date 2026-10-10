using System;
using UniversalPlatform.Features.ChapterSelect.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Presentation.Clock
{
    public class ChapterSelectViewModel
    {
        private readonly ObtenerCapitulosUseCase _obtenerCapitulos;
        private readonly ObtenerDetalleCapituloUseCase _obtenerDetalle;
        private readonly ValidarEntradaCapituloUseCase _validarEntrada;

        private readonly bool[] _bloqueados = new bool[ReglasReloj.CANTIDAD_NUMEROS];
        private int _numero;              // 0 = aún sin selección
        private float _anguloHoraria;     // acumulado

        public ChapterSelectViewState EstadoActual { get; private set; }
        public event Action<ChapterSelectViewState> OnStateChanged;
        public event Action<Capitulo> OnEntrarCapitulo;
        public event Action OnCapituloBloqueado;
        public event Action OnVolverAlMenu;

        public ChapterSelectViewModel(
            ObtenerCapitulosUseCase obtenerCapitulos,
            ObtenerDetalleCapituloUseCase obtenerDetalle,
            ValidarEntradaCapituloUseCase validarEntrada)
        {
            _obtenerCapitulos = obtenerCapitulos;
            _obtenerDetalle = obtenerDetalle;
            _validarEntrada = validarEntrada;
        }

        /// <summary>Las manillas parten en las 12 y barren hasta el número inicial.</summary>
        public void Inicializar(int numeroInicial = 1)
        {
            for (int i = 0; i < _bloqueados.Length; i++) _bloqueados[i] = true;
            foreach (var capitulo in _obtenerCapitulos.Ejecutar())
            {
                if (ReglasReloj.EsNumeroValido(capitulo.NumeroReloj))
                    _bloqueados[capitulo.NumeroReloj - 1] = !capitulo.Disponible;
            }

            _numero = 0;
            _anguloHoraria = 0f;
            Seleccionar(ReglasReloj.EsNumeroValido(numeroInicial) ? numeroInicial : 1);
        }

        /// <summary>Derecha/D/Abajo/S = +1 (sentido del reloj); Izquierda/A/Arriba/W = -1.</summary>
        public void Mover(int delta)
        {
            Seleccionar(ReglasReloj.MoverNumero(_numero, delta));
        }

        /// <summary>Mouse encima de un número.</summary>
        public void Seleccionar(int numero)
        {
            if (!ReglasReloj.EsNumeroValido(numero) || numero == _numero) return;

            float nuevoAngulo = ReglasReloj.AcumularAnguloHoraria(_anguloHoraria, numero);
            float pasos = ReglasReloj.PasosRecorridos(nuevoAngulo - _anguloHoraria);
            _anguloHoraria = nuevoAngulo;
            _numero = numero;

            EstadoActual = new ChapterSelectViewState(
                numeroSeleccionado: numero,
                detalle: _obtenerDetalle.Ejecutar(numero),
                anguloHorariaObjetivo: _anguloHoraria,
                anguloMinuteroObjetivo: ReglasReloj.AnguloMinutero(_anguloHoraria),
                pasosRecorridos: pasos,
                numeroEnLadoDerecho: ReglasReloj.EsLadoDerecho(numero),
                bloqueadosPorNumero: (bool[])_bloqueados.Clone());
            OnStateChanged?.Invoke(EstadoActual);
        }

        /// <summary>Espacio / Enter sobre el capítulo seleccionado.</summary>
        public void Confirmar()
        {
            if (!ReglasReloj.EsNumeroValido(_numero)) return;

            var resultado = _validarEntrada.Ejecutar(_numero);
            if (resultado.Estado == EstadoEntradaCapitulo.Permitida)
                OnEntrarCapitulo?.Invoke(resultado.Capitulo);
            else
                OnCapituloBloqueado?.Invoke();
        }

        /// <summary>Clic izquierdo sobre un número: lo selecciona y entra.</summary>
        public void ConfirmarNumero(int numero)
        {
            Seleccionar(numero);
            Confirmar();
        }

        /// <summary>Escape.</summary>
        public void Cancelar()
        {
            OnVolverAlMenu?.Invoke();
        }
    }
}
