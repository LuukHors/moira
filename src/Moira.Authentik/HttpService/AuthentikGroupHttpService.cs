using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Authentication;
using Moira.Authentik.Models.V3;
using Moira.Common.Exceptions;
using Moira.Common.Mappers;
using Moira.Common.Models;

namespace Moira.Authentik.HttpService;

public class AuthentikGroupHttpService(
    HttpClient client,
    IAuthentikAuthenticationService tokenService,
    ILogger<AuthentikGroupHttpService> logger) : IAuthentikHttpService<IdPGroup, AuthentikGroupV3>
{
    private const string GroupEndpoint = "api/v3/core/groups";
    private readonly Dictionary<string, object> _groupAttributes = new() {{ "managed-by", "moira" }};

    public async Task<AuthentikGroupV3?> GetAsync2(string? name, string id, IdPProvider provider, CancellationToken cancellationToken, Dictionary<string, object>? attributes = null)
    {
        var baseRequest = await AcquireBaseRequest2(provider, name, attributes ?? _groupAttributes, cancellationToken);
        
        try
        {
            if (!id.Equals(string.Empty))
            {
                return await baseRequest
                    .AppendPathSegments(id, '/')
                    .GetAsync(cancellationToken: cancellationToken)
                    .ReceiveJson<AuthentikGroupV3>();
            }

            var groups = await baseRequest
                .GetAsync(cancellationToken: cancellationToken)
                .ReceiveJson<AuthentikGroupsV3>();

            return groups.Results.FirstOrDefault();
        }
        catch (FlurlHttpException ex)
        {
            if (ex.StatusCode == 404) return null;
            var responseContent = await ex.GetResponseStringAsync();
            throw new HttpException(responseContent, null, "GET", baseRequest.Url.ToString(), ex.StatusCode);
        }
        catch
        {
            return null;
        }
    }

    public async Task<AuthentikGroupV3?> GetAsync(IdPGroup entity, CancellationToken cancellationToken)
    {
        var baseRequest = await AcquireBaseRequest(entity, cancellationToken);
        
        try
        {
            if (!entity.Status.GroupId.Equals(string.Empty))
            {
                return await baseRequest
                    .AppendPathSegments(entity.Status.GroupId, '/')
                    .GetAsync(cancellationToken: cancellationToken)
                    .ReceiveJson<AuthentikGroupV3>();
            }

            var groups = await baseRequest
                .GetAsync(cancellationToken: cancellationToken)
                .ReceiveJson<AuthentikGroupsV3>();

            return groups.Results.FirstOrDefault();
        }
        catch (FlurlHttpException ex)
        {
            if (ex.StatusCode == 404) return null;
            var responseContent = await ex.GetResponseStringAsync();
            throw new HttpException(responseContent, null, "GET", baseRequest.Url.ToString(), ex.StatusCode);
        }
        catch
        {
            return null;
        }
    }
    
    public async Task<IdPGroup> UpdateAsync(IdPGroup entity, CancellationToken cancellationToken)
    {
        var baseRequest = await AcquireBaseRequest(entity, cancellationToken);
        
        try
        {
            var parentGroupName = entity.Spec.MemberOf.FirstOrDefault();

            var parentGroup = await GetAsync2(parentGroupName, string.Empty, entity.IdPProvider, cancellationToken);
            
            var formData = new AuthentikGroupV3(entity.Spec.DisplayName,entity.Status.GroupId,[],_groupAttributes,[], parentGroup?.pk ?? string.Empty);

            var result = await baseRequest
                .AppendPathSegments(entity.Status.GroupId, '/')
                .PutJsonAsync(formData, cancellationToken: cancellationToken)
                .ReceiveJson<AuthentikGroupV3>();
            
            return entity.CopyWithNewStatus(new IdPGroupStatus(
                result.pk!,
                result.name,
                !string.IsNullOrEmpty(result.parent) ? [result.parent] : []
            ));
        }
        catch(FlurlHttpException ex)
        {
            var responseContent = await ex.GetResponseStringAsync();
            throw new HttpException(responseContent, null, "PUT", baseRequest.Url.ToString(), ex.StatusCode);
        }
    }

    public async Task<IdPGroup> CreateAsync(IdPGroup entity, CancellationToken cancellationToken)
    {
        var baseRequest = await AcquireBaseRequest(entity, cancellationToken);
        
        try
        {
            var formData = new AuthentikGroupV3(entity.Spec.DisplayName, entity.Status.GroupId, [], _groupAttributes, [], "");

            var result = await baseRequest
                .PostJsonAsync(formData, cancellationToken: cancellationToken)
                .ReceiveJson<AuthentikGroupV3>();
            
            return entity.CopyWithNewStatus(new IdPGroupStatus(
                result.pk ?? string.Empty,
                result.name
            ));
        }
        catch (FlurlHttpException ex)
        {
            var responseContent = await ex.GetResponseStringAsync();
            throw new HttpException(responseContent, null, "POST", baseRequest.Url.ToString(), ex.StatusCode);
        }
    }

    private async Task<IFlurlRequest> AcquireBaseRequest(IdPGroup entity, CancellationToken cancellationToken)
    {
        var token = await tokenService.AcquireTokenAsync(entity.IdPProvider, cancellationToken);

        var baseRequest = entity.IdPProvider.BaseUrl
            .AppendPathSegments(GroupEndpoint, '/')
            .AppendQueryParam("attributes", JsonSerializer.Serialize(_groupAttributes))
            .WithOAuthBearerToken(token);
        
        return baseRequest;
    }
    
    private async Task<IFlurlRequest> AcquireBaseRequest2(IdPProvider provider, string? name, Dictionary<string, object> attributes, CancellationToken cancellationToken)
    {
        var token = await tokenService.AcquireTokenAsync(provider, cancellationToken);

        var baseRequest = provider.BaseUrl
            .AppendPathSegments(GroupEndpoint, '/')
            .AppendQueryParam("attributes", JsonSerializer.Serialize(attributes))
            .WithOAuthBearerToken(token);
        
        if (name is not null)
            baseRequest.AppendQueryParam("name", name);
        
        return baseRequest;
    }
}