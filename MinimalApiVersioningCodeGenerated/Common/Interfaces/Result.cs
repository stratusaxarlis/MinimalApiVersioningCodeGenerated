using System.Runtime.InteropServices.JavaScript;
using MinimalApiVersioningCodeGenerated.Common.Models.Dto;

namespace MinimalApiVersioningCodeGenerated.Common.Interfaces;

public interface IResultBase
{
    bool IsSuccess { get; }
    Error Error { get; }
    object? GetValue();
}

/// <summary>Lets generic code (e.g. pipeline behaviors) build a failed Result / Result&lt;T&gt; without reflection.</summary>
public interface IResultFactory<TSelf> where TSelf : IResultFactory<TSelf>
{
    static abstract TSelf Failure(Error error);
}
