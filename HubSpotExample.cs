using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Newtonsoft.Json;
using RestSharp;

namespace HubSpotExample
{
  public static class HubSpotExample
  {
    private static string HubSpotBaseUrl = "https://api.hubapi.com";

    private static string AZURE_KEY_VAULT_URL = "[replace_with_your_key_vault_url]";
    private const string HUBSPOT_KEY_VAULT_AUTH_KEY = "[replace_with_your_key_vault_hub_spot_oauth_token_secret_name]";

    /// <summary>
    /// Gets Hubspot company based on contact email address
    /// </summary>
    /// <param name="contactEmailAddress"></param>
    public static HubSpotCompany GetCompany(string contactEmailAddress)
    {
      HubSpotContact contact = null;
      HubSpotCompany company = null;

      try
      {

        string hubSpotOAuth2Token = GetHubSpotAuthenticationTokenAsync();

        // Get the contact by email address
        contact = GetContact(contactEmailAddress,
                             hubSpotOAuth2Token);

        // Get the hubspot company
        company = GetCompany(contact.HubspotAssociactedCompanyId,
                             hubSpotOAuth2Token);


      }
      catch (Exception exceptionData)
      {
        // log it??
      }

      return company;
    }

    /// <summary>
    /// Provides the ability to retrieve the company data from HubSpot
    /// </summary>
    /// <param name="hubSpotCompanyId"></param>
    /// <param name="hubSpotOAuth2Token"></param>
    /// <returns></returns>
    private static HubSpotCompany GetCompany(long hubSpotCompanyId,
                                             string hubSpotOAuth2Token)
    {
      HubSpotCompany hubSpotCompany = new HubSpotCompany();
      string companyUrl = $"/companies/v2/companies/{hubSpotCompanyId}";
      string hubSpotCustomerActionsSemiColonDelimited = "";

      RestClient client = new RestClient(HubSpotBaseUrl + companyUrl);

      RestRequest request = new RestRequest(Method.GET);
      request.AddHeader("Authorization", $"Bearer {hubSpotOAuth2Token}");

      IRestResponse response = client.Execute(request);

      dynamic requestPayload  = JsonConvert.DeserializeObject<dynamic>(response.Content);

      hubSpotCompany.CompanyId = Convert.ToInt64(requestPayload["companyId"].ToString());
      hubSpotCustomerActionsSemiColonDelimited = requestPayload["properties"]["new_customer_actions_company"]["value"].ToString();
      if (!string.IsNullOrWhiteSpace(hubSpotCustomerActionsSemiColonDelimited))
      {
        hubSpotCompany.CustomerMilestones = hubSpotCustomerActionsSemiColonDelimited.Split(';').ToList();
      }
      else
      {
        hubSpotCompany.CustomerMilestones = new List<string>();
      }

      return hubSpotCompany;
    }

    /// <summary>
    /// Provides the ability to retrieve contact data from Hubspot
    /// </summary>
    /// <param name="contactEmailAddress"></param>
    /// <param name="hubSpotOAuth2Token"></param>
    /// <returns></returns>
    private static HubSpotContact GetContact(string contactEmailAddress,
                                             string hubSpotOAuth2Token)
    {
      HubSpotContact hubSpotContact = new HubSpotContact();
      string contactUrl = $"/contacts/v1/contact/email/{contactEmailAddress}/profile";

      RestClient client = new RestClient(HubSpotBaseUrl + contactUrl);

      RestRequest request = new RestRequest(Method.GET);

      request.AddHeader("Authorization", $"Bearer {hubSpotOAuth2Token}");

      IRestResponse response = client.Execute(request);

      dynamic requestPayload  = JsonConvert.DeserializeObject<dynamic>(response.Content);

      if (response.StatusCode == System.Net.HttpStatusCode.OK)
      {
        hubSpotContact.FirstName = requestPayload["properties"]["firstname"]["value"].ToString();
        hubSpotContact.LastName = requestPayload["properties"]["lastname"]["value"].ToString();
        hubSpotContact.Email = requestPayload["properties"]["email"]["value"].ToString();
        hubSpotContact.HubspotAssociactedCompanyId = Convert.ToInt64(requestPayload["properties"]["associatedcompanyid"]["value"].ToString());
      }

      return hubSpotContact;
    }

    /// <summary>
    /// This method will get the value from the secret setting for the specified key.
    /// </summary>
    private static string GetHubSpotAuthenticationTokenAsync()
    {
      string hubSpotAuthenticationToken = "";

      AzureServiceTokenProvider serviceTokenProvider = new AzureServiceTokenProvider();
      SecretBundle secretBundle;

      var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(serviceTokenProvider.KeyVaultTokenCallback));

      secretBundle = keyVaultClient.GetSecretAsync(AZURE_KEY_VAULT_URL,
                                                   HUBSPOT_KEY_VAULT_AUTH_KEY).Result;

      hubSpotAuthenticationToken = secretBundle.Value;

      return hubSpotAuthenticationToken;

    }
  }

  public class HubSpotCompany
  {
    public long CompanyId { get; set; }
    public List<string> CustomerMilestones { get; set; }
  }

  public class HubSpotContact
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public long HubspotAssociactedCompanyId { get; set; }
  }
}
