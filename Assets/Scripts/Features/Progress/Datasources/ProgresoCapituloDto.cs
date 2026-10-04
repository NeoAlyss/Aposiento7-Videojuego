using System;

namespace UniversalPlatform.Features.Progress.Datasources
{
    [Serializable]
    public class ProgresoCapituloDto
    {
        public string idCapitulo;
        public int llaves;
        public float segundosJugados;
        public bool completado;
    }
}
