# About
Helper class showcasing `QueryByWiqlAsync` and `UpdateWorkItemAsync` methods with explanation. With these two methods you should be able to get details on your WorkItems and update them. Utilizes PAT.

# Example

```csharp
using Azure.DevOps.Query.and.Update.WorkItems;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.Collections.Generic;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string orgName = "yourOrganizationName";
            string personalAccessToken = "yourToken";

            var queryExecutor = new QueryExecutor(orgName, personalAccessToken);

            IList<WorkItem> workItems;

            string query = "Select [ID] " +
                    "From WorkItems " +
                    "Where [State] = 'To Do' " +
                    "AND [System.TeamProject] = 'myprivateproject'";

            string[] fields = { "System.Id", "System.Title", "System.Description", "System.AssignedTo" };

            workItems = await queryExecutor.QueryWorkItems(query, fields);
            
            foreach (WorkItem workItem in workItems)
            {
                string title = workItem.Fields["System.Title"].ToString();
                string assignedTo = workItem.Fields["System.AssignedTo"].GetType().GetProperty("DisplayName").GetValue(workItem.Fields["System.AssignedTo"], null).ToString();
            }
                        
            await queryExecutor.UpdateField(workItems[0], "System.Description", "This was already done");
          }
    }
}

```

## Organization name
``https://dev.azure.com/organization/project/``

## Personal access token
How to get token -> [link](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page) 

## Query

Syntax how to write queries -> [link](https://docs.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax?view=azure-devops) 

Don't forget `"AND [System.TeamProject] = 'your project name'"` statement if you have multiple projects under one organization or you will get all workitems which satisfy your query.

##  string[] fields = { };

Reference here all field names on which you want to get information.

You can see list of all your fields here -> [link](https://docs.microsoft.com/en-us/azure/devops/boards/work-items/work-item-fields?view=azure-devops#processfields-web-page)

You can have also custom-made fields with specific formatting. In this case you will refer to them like this e.g. `Custom.VendorEmailAddress` (not System..)

:exclamation:
Situation: you have 3 WorkItems. None of them is assigned to person. The workItem object won't contain this column, so you will end up with exception. Make sure you apply some try-catch here.

:exclamation:
C# 7 supports async in Main. So if you don't have this replace:

 - workItems = **await** queryExecutor.QueryWorkItems(query, fields)
 - **await** queryExecutor.UpdateField(workItems[0], "System.Description", "This was already done")

with this:
 - workItems = queryExecutor.QueryWorkItems(query, fields)**.GetAwaiter().GetResult()**
 - queryExecutor.UpdateField(workItems[0], "System.Description", "This was already done")**.GetAwaiter().GetResult()**
