using Nethereum.JsonRpc.Client.RpcMessages;
using Nethereum.JsonRpc.Client;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

public class TrueRpcClient : RpcClient
{

    private readonly JsonSerializerSettings settings;

    private Uri uri;

    public TrueRpcClient(Uri uri) : base(uri)
    {
        this.uri = uri;
        this.settings = DefaultJsonSerializerSettingsFactory.BuildDefaultJsonSerializerSettings();
    }

    /// <inheritdoc/>
    protected override async Task<RpcResponseMessage> SendAsync(RpcRequestMessage request, string route = null)
    {
        try
        {
            var httpClient = await this.GetHttpClientAsync().ConfigureAwait(false);
            using var httpContent = new StringContent(JsonConvert.SerializeObject(request, this.settings), Encoding.UTF8, "application/json");
            var requestLog = await httpContent.ReadAsStringAsync().ConfigureAwait(false);
            //Console.WriteLine(requestLog);
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(ConnectionTimeout);

            var uri = string.IsNullOrEmpty(route) ? null : new Uri(route);

            var httpResponseMessage = await httpClient.PostAsync(uri, httpContent, cancellationTokenSource.Token).ConfigureAwait(false);
            httpResponseMessage.EnsureSuccessStatusCode();
            var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

            //begin
            //using (var reader0 = new StreamReader(stream))
            //{
            //    string responseString = await reader0.ReadToEndAsync();
            //    // Mostrar la respuesta
            //    Console.WriteLine(responseString);
            //}
            //end
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);
            var serializer = JsonSerializer.Create(this.settings);
            var message = serializer.Deserialize<RpcResponseMessage>(reader);

            return message;
        }
        catch (TaskCanceledException ex)
        {
            var exception = new RpcClientTimeoutException($"Rpc timeout after {ConnectionTimeout.TotalMilliseconds} milliseconds", ex);
            throw exception;
        }
    }

    private Task<HttpClient> GetHttpClientAsync()
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(this.GetBaseAdress());
        httpClient.DefaultRequestHeaders.ConnectionClose = true;
        var byteArray = Encoding.ASCII.GetBytes(this.uri.UserInfo);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        return Task.FromResult(httpClient);
    }


    private string GetBaseAdress()
    {
        return $"{this.uri.Scheme}://{this.uri.Authority}";
    }
}





