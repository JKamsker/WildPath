using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WildPath.Abstractions;

namespace WildPath.Extensions;

internal static class StrategyExtensions
{
    public static ISegmentStrategy Initialize(this ISegmentStrategy strat, IPathEvaluatorSegment segment)
    {
        if(strat is IParentSegmentAware aware)
        {
            aware.ParentSegment = segment;
        }

        return strat;
    }
}