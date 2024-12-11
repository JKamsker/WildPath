namespace WildPath.Abstractions;

internal interface IParentSegmentAware
{
    IPathEvaluatorSegment ParentSegment { get; set; }
}