using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeEditor.Services;

public interface IIdGenerateService
{
    string Generate(HashSet<string> existentIDs, HashSet<string> newIDs, string initialMaxId);
    string GetMaxId(HashSet<string> existentIDs, HashSet<string> newIDs, string initialMaxId);
}

public class RteIdGenerateService : IIdGenerateService
{
    const string idDigitBase = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="existentIDs">a set of RTE IDs of existent elements of template before adding new IDs</param>
    /// <param name="newIDs">a set of RTE IDs of new elements added on current commit</param>
    /// <returns></returns>
    public string Generate(HashSet<string> existentIDs, HashSet<string> newIDs, string initialMaxId)
    {
        var id = Int2Id(GetIntMaxId(existentIDs, newIDs, initialMaxId) + 1);
        newIDs.Add(id);

        return id;
    }

    public string GetMaxId(HashSet<string> existentIDs, HashSet<string> newIDs, string initialMaxId)
    {
        return Int2Id(GetIntMaxId(existentIDs, newIDs, initialMaxId));
    }

    private int GetIntMaxId(HashSet<string> existentIDs, HashSet<string> newIDs, string initialMaxId)
    {
        var maxId = string.IsNullOrEmpty(initialMaxId) ? 0 : Id2Int(initialMaxId);
        if (existentIDs.Count > 0)
            maxId = Math.Max(maxId, existentIDs.Max(o => Id2Int(o)));
        if (newIDs.Count > 0)
            maxId = Math.Max(maxId, newIDs.Max(o => Id2Int(o)));

        return maxId;
    }

    private string Int2Id(int i)
    {
        var res = "";

        var l = idDigitBase.Length;
        var n = i;
        do
        {
            var rest = n % l;
            n = n / l;
            res = idDigitBase[rest] + res;
        } while (n > 0);

        return res;
    }

    private int Id2Int(string id)
    {
        var res = 0;

        var mult = 1;
        var l = idDigitBase.Length;

        for (var i = id.Length - 1; i >= 0; i--)
        {
            res += idDigitBase.IndexOf(id[i]) * mult;
            mult *= l;
        }

        return res;
    }
}