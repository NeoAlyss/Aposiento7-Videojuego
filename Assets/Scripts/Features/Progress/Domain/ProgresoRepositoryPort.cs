namespace UniversalPlatform.Features.Progress.Domain
{
    public interface ProgresoRepositoryPort
    {
        /// <summary>Devuelve el progreso; si el capítulo nunca se jugó, uno nuevo con 0 llaves y 0 segundos.</summary>
        ProgresoCapitulo ObtenerProgreso(string idCapitulo);

        void GuardarProgreso(ProgresoCapitulo progreso);

        bool HayPartidaGuardada();

        /// <summary>Nueva partida: borra el progreso y deja una partida vacía guardada.</summary>
        void ReiniciarPartida();
    }
}
