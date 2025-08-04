using BBWM.DbDoc.DbMacros;

namespace BBWM.DbDoc.DTO;

public class DbPathMacroDTO
{
    public DbPathMacroDefinition Definition { get; set; }
    public IEnumerable<DbPathNodeDTO> Path { get; set; }
}