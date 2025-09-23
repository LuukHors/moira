using System.Net;
using System.Net.Http.Json;
using System.Text;
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
    public async Task<AuthentikGroupV3?> GetAsync(IdPGroup entity, CancellationToken cancellationToken)
    {
        var token = await tokenService.AcquireTokenAsync(entity.IdPProvider, cancellationToken);
        
        var url = BuildUrl(entity);

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        if (!entity.Status.GroupId.Equals(string.Empty))
        {
            var response = await client.GetAsync(url.ToString(), cancellationToken);

            return response.StatusCode == HttpStatusCode.NotFound 
                ? null 
                : await response.Content.ReadFromJsonAsync<AuthentikGroupV3>(cancellationToken);
        }
        
        var multipleGroupResponse = await client.GetAsync(url.ToString(), cancellationToken);
        var groups = await multipleGroupResponse.Content.ReadFromJsonAsync<AuthentikGroupsV3>(cancellationToken);
        
        return multipleGroupResponse.StatusCode == HttpStatusCode.NotFound ? null : groups?.Results.FirstOrDefault();
    }

    public async Task<IdPGroup> UpdateAsync(IdPGroup entity, CancellationToken cancellationToken)
    {
        var token = await tokenService.AcquireTokenAsync(entity.IdPProvider, cancellationToken);
        
        var url = BuildUrl(entity);

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var formData = new AuthentikGroupV3(
            entity.Spec.DisplayName,
            entity.Status.GroupId,
            new List<AuthentikUserV3>(),
            new Dictionary<string, string>
            {
                {"managed-by", "moira"}
            },
            new List<string>(),
            ""
        );

        var resultRaw = await client.PutAsJsonAsync(url.ToString(), formData, cancellationToken);
        
        try
        {
            resultRaw.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            var content = await resultRaw.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpException(content, resultRaw.StatusCode, "PUT", url.ToString());
        }
        
        var result = await resultRaw.Content.ReadFromJsonAsync<AuthentikGroupV3>(cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException("Failed to update group");
        }
        
        return entity.CopyWithNewStatus(new IdPGroupStatus(
            result.pk ?? string.Empty,
            result.name
        ));
    }

    public async Task<IdPGroup> CreateAsync(IdPGroup entity, CancellationToken cancellationToken)
    {
        var token = await tokenService.AcquireTokenAsync(entity.IdPProvider, cancellationToken);
        var url = BuildUrl(entity, isCreateAction: true);
        
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        var formData = new AuthentikGroupV3(
            entity.Spec.DisplayName,
            entity.Status.GroupId,
            new List<AuthentikUserV3>(),
            new Dictionary<string, string>
            {
                {"managed-by", "moira"}
            },
            new List<string>(),
            ""
        );
        
        var resultRaw = await client.PostAsJsonAsync(url.ToString(), formData, cancellationToken);

        try
        {
            resultRaw.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            var content = await resultRaw.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpException(content, resultRaw.StatusCode, "POST", url.ToString());
        }
        
        var result = await resultRaw.Content.ReadFromJsonAsync<AuthentikGroupV3>(cancellationToken);
        if (result is null)
        {
            throw new HttpRequestException("Failed to create group");
        }
        
        return entity.CopyWithNewStatus(new IdPGroupStatus(
            result.pk ?? string.Empty,
            result.name
        ));
    }

    private static StringBuilder BuildUrl(IdPGroup entity, bool isCreateAction = false)
    {
        var url = new StringBuilder(entity.IdPProvider.BaseUrl)
            .Append('/')
            .Append(GroupEndpoint)
            .Append('/');
            
        if(!isCreateAction)
            url.Append(entity.Status.GroupId);
            
        if(!entity.Status.GroupId.Equals(string.Empty) && !isCreateAction)
            url.Append("/?attributes={\"managed-by\":\"moira\"}");

        if(entity.Status.GroupId.Equals(string.Empty) && !isCreateAction)
        {
            url.Append("?name=").Append(entity.Spec.DisplayName).Append("&attributes={\"managed-by\":\"moira\"}");
        }
        
        return url;
    }
}