using System;
using System.Collections.Generic;

namespace UniversalPlatform.Features.Progress.Datasources
{
    [Serializable]
    public class ProgresoPartidaDto
    {
        public List<ProgresoCapituloDto> capitulos = new List<ProgresoCapituloDto>();
        public long timestampModificacion;
    }
}
