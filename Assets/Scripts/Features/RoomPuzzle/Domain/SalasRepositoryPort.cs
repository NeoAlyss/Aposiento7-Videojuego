namespace UniversalPlatform.Features.RoomPuzzle.Domain
{
    public interface SalasRepositoryPort
    {
        /// <summary>La sala del capítulo, o null si ese capítulo todavía no tiene sala.</summary>
        DefinicionSala Obtener(string idCapitulo);
    }
}
