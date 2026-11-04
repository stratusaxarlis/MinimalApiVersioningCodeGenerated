using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MinimalApiVersioningCodeGenerated.Common.Enumerators;

/// <summary>
/// Categorizes an <see cref="Models.Dto.Error"/> so it can be mapped to the appropriate HTTP response.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ErrorType>))]
public enum ErrorType
{
    [JsonStringEnumMemberName("failure")]
    [Description("A general or unexpected failure occurred.")]
    Failure = 0,

    [JsonStringEnumMemberName("validation")]
    [Description("The request failed validation.")]
    Validation = 1,

    [JsonStringEnumMemberName("not_found")]
    [Description("The requested resource was not found.")]
    NotFound = 2,

    [JsonStringEnumMemberName("conflict")]
    [Description("The request conflicts with the current state of the resource.")]
    Conflict = 3,

    [JsonStringEnumMemberName("unauthorized")]
    [Description("Authentication is required or has failed.")]
    Unauthorized = 4,

    [JsonStringEnumMemberName("forbidden")]
    [Description("The caller does not have permission to perform this action.")]
    Forbidden = 5
}
