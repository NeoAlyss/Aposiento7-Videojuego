using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UniversalPlatform.Shared
{
    /// <summary>
    /// Reproduce un video a pantalla completa (con bandas negras si no es 16:9) encima de la escena.
    /// Espacio, Enter o clic saltan al siguiente tramo del video (cada línea de diálogo empieza en
    /// uno de los <see cref="_cortes"/>); Escape la salta entera. Al terminar se desvanece y avisa.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class CinematicaView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private VideoClip _clip;
        [SerializeField] private RawImage _pantalla;
        [SerializeField] private AspectRatioFitter _ajuste;
        [Tooltip("Segundos donde empieza cada tramo (línea de diálogo). Espacio salta al siguiente; tras el último, termina.")]
        [SerializeField] private float[] _cortes = { 10.8f, 21.4f };
        [SerializeField] private float _tiempoFundido = 0.5f;

        private CanvasGroup _grupo;
        private VideoPlayer _video;
        private RenderTexture _textura;
        private Action _alTerminar;
        private bool _terminando;

        /// <summary>Está en pantalla (reproduciéndose o desvaneciéndose).</summary>
        public bool Reproduciendo { get; private set; }
        /// <summary>Frame en que terminó: la tecla que la cerró no debe contar también para la escena.</summary>
        public int FrameFin { get; private set; } = -1;

        private CanvasGroup Grupo
        {
            get
            {
                if (_grupo == null) _grupo = GetComponent<CanvasGroup>();
                return _grupo;
            }
        }

        private void Awake()
        {
            if (!Reproduciendo) Ocultar();
        }

        public void Reproducir(Action alTerminar = null)
        {
            _alTerminar = alTerminar;
            if (_clip == null)
            {
                Finalizar();
                return;
            }

            Reproduciendo = true;
            _terminando = false;
            Grupo.alpha = 1f;
            Grupo.blocksRaycasts = true;
            Grupo.interactable = true;

            _textura = new RenderTexture((int)_clip.width, (int)_clip.height, 0);
            if (_pantalla != null)
            {
                _pantalla.texture = _textura;
                _pantalla.color = Color.white;
            }
            if (_ajuste != null && _clip.height > 0) _ajuste.aspectRatio = (float)_clip.width / _clip.height;

            _video = gameObject.GetComponent<VideoPlayer>();
            if (_video == null) _video = gameObject.AddComponent<VideoPlayer>();
            _video.playOnAwake = false;
            _video.isLooping = false;
            _video.skipOnDrop = true;
            _video.source = VideoSource.VideoClip;
            _video.clip = _clip;
            _video.renderMode = VideoRenderMode.RenderTexture;
            _video.targetTexture = _textura;
            _video.audioOutputMode = VideoAudioOutputMode.None;
            _video.prepareCompleted += AlPrepararse;
            _video.loopPointReached += AlLlegarAlFinal;
            _video.Prepare();
        }

        private void AlPrepararse(VideoPlayer video) => video.Play();
        private void AlLlegarAlFinal(VideoPlayer video) => Terminar();

        private void Update()
        {
            if (!Reproduciendo) return;

            if (_terminando)
            {
                float paso = _tiempoFundido > 0f ? Time.unscaledDeltaTime / _tiempoFundido : 1f;
                Grupo.alpha = Mathf.MoveTowards(Grupo.alpha, 0f, paso);
                if (Grupo.alpha <= 0f) Finalizar();
                return;
            }

            if (MenuInput.Cancelar) Terminar();
            else if (MenuInput.Confirmar) Avanzar();
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left && Reproduciendo && !_terminando) Avanzar();
        }

        /// <summary>Salta al inicio del siguiente tramo; si ya está en el último, termina.</summary>
        private void Avanzar()
        {
            if (_video == null || !_video.isPrepared) { Terminar(); return; }
            double ahora = _video.time;
            if (_cortes != null)
                foreach (var corte in _cortes)
                    if (corte > ahora + 0.25)
                    {
                        _video.time = corte;
                        return;
                    }
            Terminar();
        }

        private void Terminar()
        {
            if (_terminando) return;
            _terminando = true;
            Grupo.blocksRaycasts = false;
            if (_video != null) _video.Pause();      // queda el último cuadro mientras se desvanece
        }

        private void Finalizar()
        {
            if (_video != null)
            {
                _video.prepareCompleted -= AlPrepararse;
                _video.loopPointReached -= AlLlegarAlFinal;
                _video.Stop();
            }
            Ocultar();
            Reproduciendo = false;
            _terminando = false;
            FrameFin = Time.frameCount;
            LiberarTextura();
            var alTerminar = _alTerminar;
            _alTerminar = null;
            alTerminar?.Invoke();
        }

        private void Ocultar()
        {
            Grupo.alpha = 0f;
            Grupo.blocksRaycasts = false;
            Grupo.interactable = false;
        }

        private void LiberarTextura()
        {
            if (_textura == null) return;
            if (_pantalla != null) _pantalla.texture = null;
            _textura.Release();
            Destroy(_textura);
            _textura = null;
        }

        private void OnDestroy() => LiberarTextura();
    }
}
