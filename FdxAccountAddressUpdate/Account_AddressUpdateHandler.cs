using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FdxAccountAddressUpdate
{
    public class Account_AddressUpdateHandler : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            int step = 0;

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity && context.Depth == 1)
            {
                try
                {
                    step = 1;
                    Entity account_context = (Entity)context.InputParameters["Target"];

                    if (account_context.LogicalName != "account")
                        return;
                
                    #region Declare and Initialize required Variables
                    step = 11;
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    //Get current user information....
                    WhoAmIResponse response = (WhoAmIResponse)service.Execute(new WhoAmIRequest());

                    step = 13;
                    string address1_line1 = null;
                    string address1_city = null;
                    string address1_country = null;
                    string fdx_zippostalcodeid = null;
                    string fdx_stateprovinceid = null;
                    string old_fdx_goldmineaccountnumber = null;
                    string companyname = null;
                    Guid fdx_zipid = Guid.Empty;
                    Guid fdx_stateid = Guid.Empty;
                    int new_fdx_gonogo = 0;
                    //Declare URL with the website link to call the Web API

                    //1. For Pointing to Dev
                    //string url = "http://smartcrmsync.1800dentist.com/api/lead/updatelead?";

                    //2. For Pointing to Stage
                    //string url = "http://smartcrmsyncstage.1800dentist.com/api/lead/updatelead?";

                    //3. For Pointing to Production
                    string url = "http://SMARTCRMSyncProd.1800dentist.com/api/lead/updatelead?";

                    string apiParm = "";
                    #endregion

                    #region Initialize variables with Context Values
                    step = 15;
                    if (account_context.Attributes.Contains("address1_line1"))
                        address1_line1 = account_context.Attributes["address1_line1"].ToString();

                    step = 17;
                    if (account_context.Attributes.Contains("address1_city"))
                        address1_city = account_context.Attributes["address1_city"].ToString();

                    step = 19;
                    if (account_context.Attributes.Contains("address1_country"))
                        address1_country = account_context.Attributes["address1_country"].ToString();

                    step = 21;
                    if (account_context.Attributes.Contains("fdx_zippostalcodeid"))
                    {
                        fdx_zippostalcodeid = (service.Retrieve("fdx_zipcode", ((EntityReference)account_context.Attributes["fdx_zippostalcodeid"]).Id, new ColumnSet("fdx_zipcode"))).Attributes["fdx_zipcode"].ToString();
                        fdx_zipid = ((EntityReference)account_context.Attributes["fdx_zippostalcodeid"]).Id;
                    }

                    step = 23;
                    if (account_context.Attributes.Contains("fdx_stateprovinceid"))
                    {
                        step = 230;
                        if (account_context.Attributes["fdx_stateprovinceid"] != null)
                        {
                            step = 2300;
                            fdx_stateprovinceid = (service.Retrieve("fdx_state", ((EntityReference)account_context.Attributes["fdx_stateprovinceid"]).Id, new ColumnSet("fdx_statecode"))).Attributes["fdx_statecode"].ToString();
                            fdx_stateid = ((EntityReference)account_context.Attributes["fdx_stateprovinceid"]).Id;
                        }
                    }

                    step = 25;
                    if (account_context.Attributes.Contains("name"))
                    {
                        companyname = account_context.Attributes["name"].ToString();
                    }
                    #endregion

                    #region Query CRM Account for required Address Attributes
                    step = 27;
                    QueryExpression queryExp = CRMQueryExpression.getQueryExpression("account", new ColumnSet("fdx_goldmineaccountnumber", "fdx_gonogo", "address1_line1", "address1_city", "address1_country", "fdx_zippostalcodeid", "fdx_stateprovinceid", "address1_line2", "address1_line3", "telephone1", "telephone2", "name"), new CRMQueryExpression[] { new CRMQueryExpression("accountid", ConditionOperator.Equal, account_context.Id) });

                    EntityCollection accountCollection = service.RetrieveMultiple(queryExp);
                    #endregion
                    step = 29;
                    //This condition will return success only if there exist an Account with the GUID in context. 
                    //Generally is successful always
                    if (accountCollection.Entities.Count > 0)
                    {
                        #region Fetch values from Account whichever do not exist in Context and create Update API String
                        step = 290;
                        Entity account = new Entity();
                        account = accountCollection.Entities[0];

                        step = 292;
                        //if (account.Attributes.Contains("fdx_gonogo"))
                        //    old_fdx_gonogo = ((OptionSetValue)account.Attributes["fdx_gonogo"]).Value;

                        step = 294;
                        if (account.Attributes.Contains("fdx_goldmineaccountnumber"))
                        {
                            old_fdx_goldmineaccountnumber = account.Attributes["fdx_goldmineaccountnumber"].ToString();
                            //Encoding the GM Account Number to resolve issue with Special Characters
                            apiParm += string.Format("&AccountNo_in={0}", WebUtility.UrlEncode(old_fdx_goldmineaccountnumber));
                        }

                        step = 296;
                        if (address1_line1 == null)
                        {
                            if (account.Attributes.Contains("address1_line1"))
                            {
                                step = 2960;
                                address1_line1 = account.Attributes["address1_line1"].ToString();
                                apiParm += string.Format("&Address1={0}", address1_line1);
                            }
                        }
                        else
                        {
                            step = 2962;
                            apiParm += string.Format("&Address1={0}", address1_line1);
                        }

                        step = 298;
                        if (address1_city == null)
                        {
                            if (account.Attributes.Contains("address1_city"))
                            {
                                step = 2980;
                                address1_city = account.Attributes["address1_city"].ToString();
                                apiParm += string.Format("&City={0}", address1_city);
                            }
                        }
                        else
                        {
                            step = 2982;
                            apiParm += string.Format("&City={0}", address1_city);
                        }

                        step = 230;
                        if (address1_country == null)
                        {
                            if (account.Attributes.Contains("address1_country"))
                            {
                                step = 2302;
                                address1_country = account.Attributes["address1_country"].ToString();
                                apiParm += string.Format("&Country={0}", address1_country);
                            }
                        }
                        else
                        {
                            step = 2304;
                            apiParm += string.Format("&Country={0}", address1_country);
                        }
                        step = 232;
                        if (fdx_zippostalcodeid == null)
                        {
                            if (account.Attributes.Contains("fdx_zippostalcodeid"))
                            {
                                step = 2320;
                                fdx_zippostalcodeid = (service.Retrieve("fdx_zipcode", ((EntityReference)account.Attributes["fdx_zippostalcodeid"]).Id, new ColumnSet("fdx_zipcode"))).Attributes["fdx_zipcode"].ToString();
                                fdx_zipid = ((EntityReference)account.Attributes["fdx_zippostalcodeid"]).Id;
                                apiParm += string.Format("&Zip={0}", fdx_zippostalcodeid);
                            }
                        }
                        else
                        {
                            step = 2322;
                            apiParm += string.Format("&Zip={0}", fdx_zippostalcodeid);
                        }
                        step = 234;
                        if (fdx_stateprovinceid == null && !account_context.Attributes.Contains("fdx_stateprovinceid"))
                        {
                            if (account.Attributes.Contains("fdx_stateprovinceid"))
                            {
                                step = 2340;
                                if (account.Attributes["fdx_stateprovinceid"] != null)
                                {
                                    step = 2342;
                                    fdx_stateprovinceid = (service.Retrieve("fdx_state", ((EntityReference)account.Attributes["fdx_stateprovinceid"]).Id, new ColumnSet("fdx_statecode"))).Attributes["fdx_statecode"].ToString();
                                    fdx_stateid = ((EntityReference)account.Attributes["fdx_stateprovinceid"]).Id;
                                    apiParm += string.Format("&State={0}", fdx_stateprovinceid);
                                }
                            }
                        }
                        else if(fdx_stateprovinceid != null)
                        {
                            step = 2344;
                            apiParm += string.Format("&State={0}", fdx_stateprovinceid);
                        }
                        step = 236;
                        if (companyname == null)
                        {
                            if (account.Attributes.Contains("name"))
                            {
                                step = 2360;
                                companyname = account.Attributes["name"].ToString();
                                apiParm += string.Format("&Company={0}", companyname);
                            }
                        }
                        else
                        {
                            step = 2362;
                            apiParm += string.Format("&Company={0}", companyname);
                        }
                        #endregion

                        #region Make a Web API Call for the Updated GoYPR scoring, and set the latest GoNogo status in to Account Context
                        step = 238;
                        url += apiParm.Remove(0, 1);

                        #region Commented in order to implement this web api call using PUT instead of POST
                        /*APIResponse accountObj = new APIResponse();
                        step = 240;
                        const string token = "8b6asd7-0775-4278-9bcb-c0d48f800112";
                        var uri = new Uri(url);
                        var request = WebRequest.Create(uri);
                        request.Method = WebRequestMethods.Http.Post;
                        request.ContentType = "application/json";
                        request.ContentLength = 0;
                        request.Headers.Add("Authorization", token);
                        step = 242;
                        using (var getResponse = request.GetResponse())
                        {
                            step = 2420;
                            DataContractJsonSerializer serializer =
                                        new DataContractJsonSerializer(typeof(APIResponse));

                            accountObj = (APIResponse)serializer.ReadObject(getResponse.GetResponseStream());
                            step = 2422;
                            account_context["fdx_gonogo"] = accountObj.goNoGo ? new OptionSetValue(756480000) : new OptionSetValue(756480001);
                            step = 2424;
                            new_fdx_gonogo = accountObj.goNoGo ? 756480000 : 756480001;
                        }*/
                        #endregion

                        API_PutResponse accountObj = new API_PutResponse();
                        step = 240;
                        const string token = "8b6asd7-0775-4278-9bcb-c0d48f800112";
                        var uri = new Uri(url);
                        var request = WebRequest.Create(uri);
                        request.Method = WebRequestMethods.Http.Put;
                        request.ContentType = "application/json";
                        request.ContentLength = 0;
                        request.Headers.Add("Authorization", token);
                        step = 242;
                        using (var getResponse = request.GetResponse())
                        {
                            step = 2420;
                            DataContractJsonSerializer serializer =
                                        new DataContractJsonSerializer(typeof(API_PutResponse));

                            accountObj = (API_PutResponse)serializer.ReadObject(getResponse.GetResponseStream());
                            step = 2422;
                            account_context["fdx_gonogo"] = accountObj.goNoGo ? new OptionSetValue(756480000) : new OptionSetValue(756480001);
                            step = 2424;
                            new_fdx_gonogo = accountObj.goNoGo ? 756480000 : 756480001;
                        }
                        #endregion

                        #region Select Leads related to Account and Update the latest GoNogo Status and Address attributes
                        step = 244;
                        if (new_fdx_gonogo != 0 && old_fdx_goldmineaccountnumber != null)
                        {
                            step = 2440;
                            QueryExpression leadQuery = CRMQueryExpression.getQueryExpression("lead", new ColumnSet("fdx_gonogo", "parentaccountid"), new CRMQueryExpression[] { new CRMQueryExpression("fdx_goldmineaccountnumber", ConditionOperator.Equal, old_fdx_goldmineaccountnumber) });
                            step = 2442;
                            EntityCollection leadEntities = service.RetrieveMultiple(leadQuery);
                            step = 2444;
                            for (int i = 0; i < leadEntities.Entities.Count; i++)
                            {
                                step = 24440;
                                if (leadEntities.Entities[i].Attributes.Contains("parentaccountid"))
                                {
                                    step = 244400;
                                    if (((EntityReference)leadEntities.Entities[i].Attributes["parentaccountid"]).Id == account_context.Id)
                                    {
                                        step = 2444000;                                        
                                        Entity lead = new Entity("lead")
                                        {
                                            Id = leadEntities.Entities[i].Id
                                        };
                                        step = 2444002;
                                        lead["fdx_gonogo"] = new OptionSetValue(new_fdx_gonogo);
                                        step = 2444004;
                                        lead["address1_line1"] = address1_line1;
                                        step = 2444006;
                                        lead["address1_city"] = address1_city;
                                        step = 2444008;
                                        lead["address1_country"] = address1_country;
                                        step = 2444010;
                                        if (fdx_stateid != Guid.Empty)
                                        {
                                            lead["fdx_stateprovince"] = new EntityReference("fdx_state", fdx_stateid);
                                        }
                                        else
                                        {
                                            lead["fdx_stateprovince"] = null;
                                        }
                                        step = 2444012;
                                        lead["fdx_zippostalcode"] = new EntityReference("fdx_zipcode", fdx_zipid);
                                        step = 2444014;
                                        lead["companyname"] = companyname;
                                        step = 2444016;
                                        if (account_context.Attributes.Contains("address1_line2"))
                                        {
                                            step = 24440160;
                                            lead["address1_line2"] = account_context.Attributes["address1_line2"].ToString();
                                        }
                                        else if (account.Attributes.Contains("address1_line2"))
                                        {
                                            step = 24440162;
                                            lead["address1_line2"] = account.Attributes["address1_line2"].ToString();
                                        }
                                        else
                                        {
                                            step = 24440164;
                                            lead["address1_line2"] = null;
                                        }
                                        step = 2444018;
                                        if (account_context.Attributes.Contains("address1_line3"))
                                        {
                                            step = 24440180;
                                            lead["address1_line3"] = account_context.Attributes["address1_line3"].ToString();
                                        }
                                        else if (account.Attributes.Contains("address1_line3"))
                                        {
                                            step = 24440182;
                                            lead["address1_line3"] = account.Attributes["address1_line3"].ToString();
                                        }
                                        else
                                        {
                                            step = 24440184;
                                            lead["address1_line3"] = null;
                                        }
                                        step = 2444020;
                                        if (account_context.Attributes.Contains("telephone1"))
                                        {
                                            step = 24440200;
                                            lead["telephone2"] = Regex.Replace(account_context.Attributes["telephone1"].ToString(),@"[^0-9]+", "");
                                            //lead["telephone2"] = account_context.Attributes["telephone1"].ToString();
                                        }
                                        else if (account.Attributes.Contains("telephone1"))
                                        {
                                            step = 24440202;
                                            lead["telephone2"] = Regex.Replace(account.Attributes["telephone1"].ToString(),@"[^0-9]+", "");
                                            //lead["telephone2"] = account.Attributes["telephone1"].ToString();
                                        }
                                        else
                                        {
                                            step = 24440204;
                                            lead["telephone2"] = null;
                                        }
                                        step = 2444022;
                                        if (account_context.Attributes.Contains("telephone2"))
                                        {
                                            step = 24440220;
                                            lead["telephone3"] = Regex.Replace(account_context.Attributes["telephone2"].ToString(),@"[^0-9]+", "");
                                            //lead["telephone3"] = account_context.Attributes["telephone2"].ToString();
                                        }
                                        else if (account.Attributes.Contains("telephone2"))
                                        {
                                            step = 24440222;
                                            lead["telephone3"] = Regex.Replace(account.Attributes["telephone2"].ToString(),@"[^0-9]+", "");
                                            //lead["telephone3"] = account.Attributes["telephone2"].ToString();
                                        }
                                        else
                                        {
                                            step = 24440224;
                                            lead["telephone3"] = null;
                                        }
                                        //Holds some value in this field, to indicate that the Lead is updated by Account Plugin and not by Form
                                        step = 2444024;
                                        lead["fdx_accountcontext"] = "1";
                                        step = 2444026;
                                        service.Update(lead);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Select Opportunities related to Account and Update the latest GoNogo Status
                        step = 246;
                        if (new_fdx_gonogo != 0 && old_fdx_goldmineaccountnumber != null)
                        {
                            step = 2460;
                            QueryExpression opportunityQuery = CRMQueryExpression.getQueryExpression("opportunity", new ColumnSet("fdx_gonogo", "parentaccountid"), new CRMQueryExpression[] { new CRMQueryExpression("fdx_goldmineaccountnumber", ConditionOperator.Equal, old_fdx_goldmineaccountnumber) });
                            step = 2462;
                            EntityCollection opportunityEntities = service.RetrieveMultiple(opportunityQuery);
                            step = 2464;
                            for (int i = 0; i < opportunityEntities.Entities.Count; i++)
                            {
                                step = 24640;
                                if (opportunityEntities.Entities[i].Attributes.Contains("parentaccountid"))
                                {
                                    step = 246400;
                                    if (((EntityReference)opportunityEntities.Entities[i].Attributes["parentaccountid"]).Id == account_context.Id)
                                    {
                                        step = 2464000;
                                        Entity opportunity = new Entity("opportunity")
                                        {
                                            Id = opportunityEntities.Entities[i].Id
                                        };
                                        step = 2464002;
                                        opportunity["fdx_gonogo"] = new OptionSetValue(new_fdx_gonogo);
                                        step = 2464004;
                                        service.Update(opportunity);
                                    }
                                }
                            }
                        }
                        step = 248;
                        #endregion
                        //Thread.Sleep(120000);
                        //System.Threading.Thread.Sleep(new TimeSpan(0, 1, 0));
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException(string.Format("An error occurred in the Updating Go/No-go Status to Account in plug-in at Step {0}. " + ex.Message, step), ex);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("Account_AddressUpdateHandler: step {0}, {1}", step, ex.ToString());
                    throw;
                }
            }
        }
    }
}
