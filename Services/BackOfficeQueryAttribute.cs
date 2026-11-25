using Microsoft.AspNetCore.Authorization;

namespace EnjoySpace.Web.Services;

[AttributeUsage(AttributeTargets.Method)]
public sealed class BackOfficeQueryAttribute : Attribute
{
    public static bool Allows(AuthorizationHandlerContext context) =>
        context.User.IsInRole(StaffAccess.ReadOnlyRole)
        && context.Resource is HttpContext http
        && (HttpMethods.IsGet(http.Request.Method) || HttpMethods.IsHead(http.Request.Method))
        && http.GetEndpoint()?.Metadata.GetMetadata<BackOfficeQueryAttribute>() is not null;
}
