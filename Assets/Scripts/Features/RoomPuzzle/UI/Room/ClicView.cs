using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UniversalPlatform.Features.RoomPuzzle.UI.Room
{
    /// <summary>Reenvía un clic izquierdo (lo usa el velo de la narradora para pasar de texto).</summary>
    public class ClicView : MonoBehaviour, IPointerClickHandler
    {
        public event Action AlClic;

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) AlClic?.Invoke();
        }
    }
}
