using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.DevOps.Query.and.Update.WorkItems
{
    public class QueryExecutor
    {
        private readonly string _orgName;
        private readonly string _personalAccessToken;
        private readonly Uri _uri;
        private readonly VssBasicCredential _credentials;
        private readonly WorkItemTrackingHttpClient _httpClient;
        private readonly JsonPatchDocument _jsonPatchDocument;

        public QueryExecutor(string orgName, string personalAccessToken)
        {
            _orgName = orgName;
            _personalAccessToken = personalAccessToken;
            _uri = new Uri("https://dev.azure.com/" + _orgName);
            _credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
            _httpClient = new WorkItemTrackingHttpClient(_uri, _credentials);
            _jsonPatchDocument = new JsonPatchDocument();
        }

        public async Task<IList<WorkItem>> QueryWorkItems(string query, string[] fields)
        {

            var wiql = new Wiql()
            {
                Query = query
            };

            var result = await _httpClient.QueryByWiqlAsync(wiql).ConfigureAwait(false);
            var ids = result.WorkItems.Select(item => item.Id).ToArray();

            if (ids.Length == 0)
            {
                return Array.Empty<WorkItem>();
            }

            return await _httpClient.GetWorkItemsAsync(ids, fields, result.AsOf).ConfigureAwait(false);
        }

        public async Task UpdateField(WorkItem workitem, string field, string value)
        {
            _jsonPatchDocument.Clear();
            _jsonPatchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Replace,
                    Path = "/fields/" + field,
                    Value = value
                }
            );

            await _httpClient.UpdateWorkItemAsync(_jsonPatchDocument, Convert.ToInt32(workitem.Id)).ConfigureAwait(false);

        }
    }
}
