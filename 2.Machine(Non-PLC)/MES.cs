using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;

namespace service;
public class MES
{
    public HttpClient MESConnection { get; set; }
    public string MESDatavalue { get; set; } = "";
    public string BusinessUnit { get; set; }
    public string ShopfloorUrl { get; set; }
    public bool HttpsFlag { get; set; }

    public MES()
    {

    }

    public void Connect(string url = null)
    {
        url ??= ShopfloorUrl;
        var handler = new HttpClientHandler();

        if (HttpsFlag)
        {
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        }

        MESConnection = new HttpClient(handler)
        {
            BaseAddress = new Uri($"https://{url}")
        };
    }


    public async Task<bool> RequestDataAsync(string url, HttpMethod method)
    {
        try
        {
            if (MESConnection != null)
            {
                var request = new HttpRequestMessage(method, url);
                var response = await MESConnection.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    MESDatavalue = await response.Content.ReadAsStringAsync();
                    return true;
                }
            }
        }
        catch
        {

        }
        return false;
    }

    public string GetData() => MESDatavalue;
}