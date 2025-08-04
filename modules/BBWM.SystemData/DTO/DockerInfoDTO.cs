namespace BBWM.SystemData.DTO;

public class DockerInfoDTO
{
    public string Arn { get; set; }

    public string DesiredStatus { get; set; }

    public string Family { get; set; }

    public string KnownStatus { get; set; }

    public string Version { get; set; }

    public List<DockerContainerInfoDTO> Containers { get; set; }
}
