using CipAdminCenter.Api.Contracts;
using CipAdminCenter.Application.Organisations;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Api.Endpoints;

internal static class EndpointHelpers
{
    public static ExternalReferenceDto? ToDto(this ExternalReferenceRequest? r) => r is null ? null : new(r.Scheme, r.Identifier);

    public static bool TryParseLevel(string level, out ContextLevel result) =>
        Enum.TryParse(level, ignoreCase: true, out result) && Enum.IsDefined(result);

    public static IResult InvalidLevel(string level) =>
        Results.Problem($"'{level}' is not a context level. Use tenant, facility or unit.", statusCode: StatusCodes.Status400BadRequest);
}
