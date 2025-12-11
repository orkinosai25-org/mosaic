using Microsoft.AspNetCore.Mvc;
using OrkinosaiCMS.Core.Interfaces.Services;

namespace OrkinosaiCMS.Web.Controllers;

/// <summary>
/// API Controller for handling Stripe webhooks
/// </summary>
[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IStripeService stripeService,
        ILogger<WebhookController> logger)
    {
        _stripeService = stripeService;
        _logger = logger;
    }

    /// <summary>
    /// Handle Stripe webhook events
    /// </summary>
    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        try
        {
            // Enable buffering so the request body can be read multiple times
            HttpContext.Request.EnableBuffering();
            
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

            if (string.IsNullOrEmpty(stripeSignature))
            {
                _logger.LogWarning("Webhook received without Stripe signature");
                return BadRequest("Missing Stripe signature");
            }

            // Verify webhook signature
            if (!_stripeService.VerifyWebhookSignature(json, stripeSignature))
            {
                _logger.LogWarning("Webhook signature verification failed");
                return BadRequest("Invalid signature");
            }

            // Parse event type
            var eventType = Newtonsoft.Json.Linq.JObject.Parse(json)["type"]?.ToString();
            if (string.IsNullOrEmpty(eventType))
            {
                _logger.LogWarning("Webhook received without event type");
                return BadRequest("Missing event type");
            }

            _logger.LogInformation("Processing Stripe webhook event: {EventType}", eventType);

            // Process the webhook event
            await _stripeService.ProcessWebhookEventAsync(eventType, json);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500, "Error processing webhook");
        }
    }
}
