using MyYt.Application.Abstractions;

namespace MyYt.Api.Infrastructure;

internal sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id =>
        accessor.HttpContext?.Items[UserProvisioningMiddleware.UserIdKey] is Guid id
            ? id
            : throw new InvalidOperationException("No authenticated user on this request.");
}
