
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ContosoCrafts.Web.Server.Hubs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ContosoCrafts.Web.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using ContosoCrafts.Web.Server.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ContosoCrafts.Web.Server.Controllers
{
    [Route("api/checkout")]
    public class CheckoutController : ControllerBase
    {
        private readonly IHubContext<EventsHub> eventsHub;
        private readonly ILogger<CheckoutController> logger;
        private readonly IConfiguration configuration;
        private readonly IProductService productService;
        private readonly IDistributedCache cache;

        public CheckoutController(IProductService productService,
                                  IHubContext<EventsHub> eventsHub,
                                  IConfiguration configuration,
                                  IDistributedCache cache,
                                  ILogger<CheckoutController> logger)
        {
            this.cache = cache;
            this.productService = productService;
            this.logger = logger;
            this.eventsHub = eventsHub;
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult> CheckoutOrder([FromBody]IEnumerable<CartItem> items, [FromServices] IServiceProvider sp)
        {
            logger.LogInformation("Order received...");

            // Build the URL to which the customer will be redirected after paying.
            var host = $"{Request.Scheme}://{Request.Host.ToString()}";
            var server = sp.GetRequiredService<IServer>();
            var callbackRoot = server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();

            var checkoutResponse = await productService.CheckOut(items, callbackRoot);
            await eventsHub.Clients.All.SendAsync("CheckoutSessionStarted", checkoutResponse);
            return Ok();
        }

        [Route("config")]
        public ConfigResponse Config()
        {
            return new ConfigResponse
            {
                StripePublicKey = configuration["Stripe:PubKey"]
            };
        }
    }
}
