using System.Collections.Generic;

namespace RuntimeEditor.Services;

public class RteIdSet
{
    private readonly HashSet<string> origIdSet;
    private readonly HashSet<string> newIdSet;

    public RteIdSet(HashSet<string> origIdSet, string initialMaxId)
    {
        newIdSet = new HashSet<string>();

        this.origIdSet = origIdSet ?? new HashSet<string>();
        InitialMaxId = initialMaxId;
    }

    public string GenerateNewId()
    {
        var id = new RteIdGenerateService().Generate(origIdSet, newIdSet, InitialMaxId);
        newIdSet.Add(id);
        return id;
    }

    public string InitialMaxId { get; }

    public string GetNewMaxId()
    {
        return new RteIdGenerateService().GetMaxId(origIdSet, newIdSet, InitialMaxId);
    }
}