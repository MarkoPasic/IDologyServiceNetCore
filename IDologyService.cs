using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Adapters.IDology.Helpers;
using Adapters.IDology.Types.ExpectId;
using Newtonsoft.Json;
using System.Net.Http;

public class IDologyService
{
    private readonly IRequestFactory _requestFactory;
    private readonly string[] _baseParams;
    private readonly IConfiguration _config;

    public IDologyService(IRequestFactory requestFactory, IConfiguration config)
    {
        _requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        _baseParams = InitializeBaseParams();
    }

    private string[] InitializeBaseParams()
    {
        string username = _config["IDology:Username"];
        string password = _config["IDology:Password"];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Username or Password is not configured properly.");
        }

        return new string[] { username, password, "JSON" };
    }

    public async Task<ExpectIdResponse> ExpectID(string[] args)
    {
        return await CallApi("/api/idiq.svc", new string[] {
            "firstName", "lastName", "address", "city", "state", "zip", "country", "ssn"
        }, args);
    }

    public async Task<ExpectIdResponse> ExpectIDEmail(string[] args) // Just for testing, ExpectIDEmail can be tested with ExpectID call with additional param email
    {
        return await CallApi("/api/idiq.svc", new string[] {
            "firstName", "lastName", "address", "city", "state", "zip", "email"
        }, args);
    }

    public async Task<ExpectIdResponse> EmailStandalone(string[] args)
    {
        return await CallApi("/api/email-standalone.svc", new string[] {
            "invoice", "firstName", "lastName", "email"
        }, args);
    }

    public async Task<ExpectIdResponse> ExpectIDGeoTrace(string[] args)
    {
        return await CallApi("/api/idiq.svc", new string[] {
            "firstName", "lastName", "address", "city", "state", "zip", "ipAddress"
        }, args);
    }

    public async Task<ExpectIdResponse> AlertList(string[] args)
    {
        return await CallApi("/api/alert-list.svc", new string[] {
            "alertlist", "fieldToCheck", "valueToBlock", "alertlist", "action"
        }, args);
    }

    public async Task<ExpectIdResponse> ExpectIdPA(string[] args)
    {
        return await CallApi("/api/pa-standalone.svc", new string[] {
            "firstName", "lastName", "address", "city", "state", "zip"
        }, args);
    }

    public async Task<ExpectIdResponse> ExpectIdScanOnboard(string[] args)
    {
        return await CallApi("/api/idscanperform.svc", new string[] {
            "queryId","image","backImage","countryCode","scanDocumentType","faceImage","ipAddress"
        }, args);
    }

    public async Task<ExpectIdResponse> ExpectIdScanVerify(string[] args)
    {
        return await CallApi("/api/idscanperform.svc", new string[] {
            "queryId","image","backImage","countryCode","scanDocumentType","faceImage","ipAddress"
        }, args);
    }

    public async Task<ExpectIdResponse> ExpectIdAlertList(string[] args)
    {
        return await CallApi("/api/alert-list.svc", new string[] {
             "alertlist", "fieldToCheck", "valueToBlock", "alertlist", "action"
        }, args);
    }

    private async Task<ExpectIdResponse> CallApi(string endpoint, string[] requiredParams, string[] args)
    {
        var expectIdRequest = _requestFactory.CreateRequest(endpoint, requiredParams);
        var request = await expectIdRequest.CallAPIAsync(BuildParams(args));
        return await ParseResponse(request);
    }

    private string[] BuildParams(params string[] newParams)
    {
        int newSize = _baseParams.Length + newParams.Length;
        string[] updatedParams = new string[newSize];

        Array.Copy(_baseParams, updatedParams, _baseParams.Length);

        for (int i = 0; i < newParams.Length; i++)
        {
            updatedParams[_baseParams.Length + i] = newParams[i];
        }

        return updatedParams;
    }

    private async Task<ExpectIdResponse> ParseResponse(HttpResponseMessage request)
    {
        try
        {
            string responseBody = await request.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ExpectIdResponse>(responseBody);
        }
        catch (Exception ex)
        {
            throw new Exception("Error: " + ex.Message);
        }
    }
}
