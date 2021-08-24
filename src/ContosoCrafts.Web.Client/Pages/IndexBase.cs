using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.Toast.Services;
using ContosoCrafts.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.Extensions.Configuration;

namespace ContosoCrafts.Web.Client.Pages
{
    public class IndexBase : ComponentBase, IAsyncDisposable
    {
        [Inject]
        private IHttpClientFactory ClientFactory { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [Inject]
        private ILogger<IndexBase> logger { get; set; }

        [Inject]
        private IToastService toastService { get; set; }

        [Inject]
        private IConfiguration Configuration { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            logger.LogInformation("OnInitializedAsync called");

            hubConnection = new HubConnectionBuilder()
                        .WithUrl(NavigationManager.ToAbsoluteUri("/events"))
                        .Build();

            hubConnection.On<CheckoutResponse>("CheckoutSessionStarted", async (chkResp) =>
            {
                logger.LogInformation("CheckoutSessionStarted fired");
                await JSRuntime.InvokeVoidAsync("checkout", chkResp.PaymentIntentClientSecret);
            });

            await hubConnection.StartAsync();
        }

        protected override async void OnAfterRender(bool firstRender)
        {
            if(firstRender)
            {
                var client = ClientFactory.CreateClient("localapi");
                var config = await client.GetFromJsonAsync<ConfigResponse>("/api/checkout/config");
                await JSRuntime.InvokeVoidAsync("registerElements", config.StripePublicKey);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await hubConnection.DisposeAsync();
        }
    }
}
