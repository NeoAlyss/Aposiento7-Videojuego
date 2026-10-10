using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UniversalPlatform.Features.RoomPuzzle.Domain;

namespace UniversalPlatform.Features.RoomPuzzle.UI.Room
{
    /// <summary>Una baldosa del suelo: avisa cuando le hacen clic para caminar hacia ella.</summary>
    public class BaldosaView : MonoBehaviour, IPointerClickHandler
    {
        private Celda _celda;
        private Action<Celda> _alClic;

        public void Construir(Celda celda, Action<Celda> alClic)
        {
            _celda = celda;
            _alClic = alClic;
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) _alClic?.Invoke(_celda);
        }
    }
}
