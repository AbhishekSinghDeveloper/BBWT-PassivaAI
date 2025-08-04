namespace BBWM.Core.ModelHashing;

public class KeysMap
{
    public Type ModelType { get; set; }

    public string Property { get; set; }

    public byte[] Salt { get; set; }
}
