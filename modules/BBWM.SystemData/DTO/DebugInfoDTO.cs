namespace BBWM.SystemData.DTO;

public class DebugInfoDTO
{
    public IEnumerable<KeyValuePair<string, string>> LinkedModules { get; set; }
    public IEnumerable<OutputExceptionDTO> LinkerInvokeExceptions { get; set; }
    public IEnumerable<OutputExceptionDTO> LinkerCommonExceptions { get; set; }
    public IEnumerable<OutputExceptionDTO> ApiExceptions { get; set; }
}