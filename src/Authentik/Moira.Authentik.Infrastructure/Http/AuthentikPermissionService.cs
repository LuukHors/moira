using Flurl;
using Flurl.Http;
using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain;
using Moira.Authentik.Domain.Roles;
using Moira.Authentik.Infrastructure.Authentication;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Infrastructure.Http;

public class AuthentikPermissionService(IAuthentikAuthenticationService authService) : IAuthentikPermissionService
{
    public async Task<AuthentikPageResult<AuthentikPermissionV3>> GetPermissionsAsync(string roleId, IdPProvider provider, CancellationToken cancellationToken)
    {
        try
        {
            var token = await authService.AcquireTokenAsync(provider, cancellationToken);

            var request = provider.BaseUrl
                .AppendPathSegment("api/v3/rbac/permissions/")
                .AppendQueryParam("role", roleId)
                .WithOAuthBearerToken(token)
                .WithHeader("Accept", "application/json");
            
            var permissions = await request.GetAsync(cancellationToken: cancellationToken)
                .ReceiveJson<AuthentikPageResult<AuthentikPermissionV3>>();

            return permissions;
        }
        catch (FlurlHttpException ex)
        {
            var body = await ex.GetResponseStringAsync();
            throw new IdPException(body, IdPExceptionReason.IdpRequestFailed, ex);
        }
    }

    public async Task<bool> UnassignPermissionsAsync(string roleId, IEnumerable<string> permissions, IdPProvider provider, CancellationToken cancellationToken)
    {
        try
        {
            var token = await authService.AcquireTokenAsync(provider, cancellationToken);

            var request = provider.BaseUrl
                .AppendPathSegment("api/v3/rbac/permissions/assigned_by_roles/")
                .AppendPathSegment(roleId)
                .AppendPathSegment("/unassign/")
                .WithOAuthBearerToken(token)
                .WithHeader("Accept", "application/json");

            await request.PatchJsonAsync(body: new { permissions }, cancellationToken: cancellationToken);

            return true;
        }
        catch (FlurlHttpException ex)
        {
            var body = await ex.GetResponseStringAsync();
            throw new IdPException(body, IdPExceptionReason.IdpRequestFailed, ex);
        }
    }

    public async Task<bool> AssignPermissionsAsync(string roleId, IEnumerable<string> permissions, IdPProvider provider, CancellationToken cancellationToken)
    {
        try
        {
            var token = await authService.AcquireTokenAsync(provider, cancellationToken);

            var request = provider.BaseUrl
                .AppendPathSegment("api/v3/rbac/permissions/assigned_by_roles/")
                .AppendPathSegment(roleId)
                .AppendPathSegment("/assign/")
                .WithOAuthBearerToken(token)
                .WithHeader("Content-Type", "application/json");

            await request.PostJsonAsync(body: new { permissions }, cancellationToken: cancellationToken);

            return true;
        }
        catch (FlurlHttpException ex)
        {
            var body = await ex.GetResponseStringAsync();
            throw new IdPException(body, IdPExceptionReason.IdpRequestFailed, ex);
        }
    }
}