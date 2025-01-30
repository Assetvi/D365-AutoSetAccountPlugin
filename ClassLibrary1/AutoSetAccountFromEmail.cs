using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

public class AutoSetAccountFromEmail : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        // Obtain the execution context  
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

        // Ensure the plugin is triggered on "Create" of Contact  
        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
        {
            Entity contact = (Entity)context.InputParameters["Target"];

            // Ensure email field exists  
            if (contact.Contains("emailaddress1"))
            {
                string email = contact["emailaddress1"].ToString();
                string domain = email.Split('@').Length > 1 ? email.Split('@')[1] : null;

                if (!string.IsNullOrEmpty(domain))
                {
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    // Find an existing Account with the domain in their website field  
                    QueryExpression query = new QueryExpression("account")
                    {
                        ColumnSet = new ColumnSet("accountid", "name"),
                        Criteria = new FilterExpression()
                        {
                            Conditions =
                            {
                                new ConditionExpression("websiteurl", ConditionOperator.Like, "%" + domain + "%")
                            }
                        }
                    };

                    EntityCollection accounts = service.RetrieveMultiple(query);

                    if (accounts.Entities.Count > 0)
                    {
                        contact["parentcustomerid"] = new EntityReference("account", accounts.Entities[0].Id);
                    }
                }
            }
        }
    }
}

