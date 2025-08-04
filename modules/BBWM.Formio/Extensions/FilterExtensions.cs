using BBWM.Core.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBWM.FormIO.Extensions;

public static class FilterExtensions
{
    public static TFilter? GetFilter<TFilter>(this Filter filter, string fieldName, bool removeFilter = true) where TFilter : FilterInfoBase
    {
        if(filter?.Filters.FirstOrDefault(f => string.Compare(fieldName, f.PropertyName, true) == 0) is TFilter result)
        {
            if(removeFilter)
            {
                _ = filter.Filters.Remove(result);
            }

            return result;
        }

        return default;
    }

}

