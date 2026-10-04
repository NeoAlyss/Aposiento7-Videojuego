using TMPro;
using UnityEngine;
using UniversalPlatform.Features.Progress.Presentation.ChapterTimer;

namespace UniversalPlatform.Features.Progress.UI.ChapterTimer
{
    /// <summary>
    /// Ponlo en la escena de cada capítulo. Suma el tiempo jugado (no cuenta en pausa) y lo guarda.
    /// Desde tu lógica de juego llama a AgregarLlave() al recoger una llave y a Completar() al terminar.
    /// El texto es opcional (para mostrar el cronómetro en pantalla).
    /// </summary>
    public class ChapterTimerView : MonoBehaviour
    {
        [SerializeField] private string _idCapitulo = "chapter_01";
        [SerializeField] private TMP_Text _textoTiempo;

        private ChapterTimerViewModel _viewModel;

        public string IdCapitulo => _idCapitulo;

        public void Construir(ChapterTimerViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.OnStateChanged += AplicarEstado;
            _viewModel.Inicializar(_idCapitulo);
        }

        private void Update()
        {
            if (_viewModel == null || Time.timeScale <= 0f) return;
            _viewModel.AvanzarTiempo(Time.unscaledDeltaTime);
        }

        public void AgregarLlave(int cantidad = 1) => _viewModel?.AgregarLlave(cantidad);
        public void Completar() => _viewModel?.Completar();

        private void AplicarEstado(ChapterTimerViewState estado)
        {
            if (_textoTiempo != null) _textoTiempo.text = estado.TiempoFormateado;
        }

        private void OnApplicationPause(bool pausada)
        {
            if (pausada) _viewModel?.Finalizar();
        }

        private void OnDestroy()
        {
            if (_viewModel == null) return;
            _viewModel.OnStateChanged -= AplicarEstado;
            _viewModel.Finalizar();
        }
    }
}
