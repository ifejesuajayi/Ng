using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xown.Travels.Core;

namespace _247.Travels.Ng.Apis.Server
{
    /// <summary>
    /// Manages standard Web API for flight shopping
    /// </summary>
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Xown.Travels.Core.AuthorizationPolicy.Customer)]
    public class ShoppingController : ControllerBase
    {
        #region Private Members

        /// <summary>
        /// Singleton instance of the <see cref="ILogger"/>
        /// </summary>
        private readonly ILogger<ShoppingController> logger;

        /// <summary>
        /// The scoped instance of the <see cref="ShoppingOperations"/>
        /// </summary>
        private readonly ShoppingOperations shoppingOperations;

        /// <summary>
        /// The scoped instance of the <see cref="DistributionService"/>
        /// </summary>
        private readonly DistributionService distributionService;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ShoppingController(ShoppingOperations shoppingOperations,
            ILogger<ShoppingController> logger,
            DistributionService distributionService)
        {
            this.logger = logger;
            this.shoppingOperations = shoppingOperations;
            this.distributionService = distributionService;
        }

        #endregion

        /// <summary>
        /// Processes flight offers
        /// </summary>
        /// <param name="flightRequestId">The flight request id</param>
        /// <param name="sessionId">The session or cache id</param>
        /// <returns>Flight results</returns>
        [AllowAnonymous]
        [HttpGet(EndpointRoutes.FetchFlightOffers)]
        public async Task<ActionResult> GetOffersAsync([FromQuery] string flightRequestId, [FromQuery] string sessionId)
        {
            // Fire the transaction
            var transaction = await shoppingOperations.GetOffersAsync(flightRequestId, sessionId);

            // If transaction failed...
            if (!transaction.Successful)
            {
                // Return the problem
                return Problem(
                    title: transaction.ErrorTitle,
                    detail: transaction.ErrorMessage,
                    statusCode: transaction.StatusCode);
            }

            return Ok(transaction.Result);
        }

        /// <summary>
        /// Initiates the operation that processes flight offers
        /// </summary>
        /// <param name="flightRequestId">The flight request id</param>
        /// <param name="amaClientRef">The ama client ref</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet(EndpointRoutes.ProcessFlightOffers)]
        public async Task<ActionResult> ProcessOffersAsync([FromQuery] string flightRequestId)
        {
            try
            {
                // Process flight offers
                var operation = await distributionService.ProcessOffersAsync(flightRequestId);

                // If operation was un successful...
                if (!operation.Successful)
                {
                    // Log the error
                    logger.LogError(operation.ErrorMessage);

                    // Return error response
                    return Problem(title: operation.ErrorTitle,
                        statusCode: operation.StatusCode, detail: operation.ErrorMessage);
                }

                // Return result
                return Ok(operation.Result);
            }
            catch (Exception ex)
            {
                // Log error response
                logger.LogError(ex.Message);

                // Return error response
                return Problem(title: "SYSTEM ERROR",
                    statusCode: StatusCodes.Status500InternalServerError, detail: ex.Message);
            }
        }

        /// <summary>
        /// Verifies price of a flight result
        /// </summary>
        /// <param name="offerId">The flight offer id</param>
        /// <param name="amaClientRef">The ama client ref</param>
        /// <param name="includedFareRules">True, if fare should be included</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet(EndpointRoutes.FetchFlightOfferPricing)]
        public async Task<ActionResult> VerifyPriceAsync([FromQuery] string offerId, [FromQuery] string officeId, [FromQuery] string amaClientRef, [FromQuery] bool includedFareRules)
        {
            // Fire the transaction
            var transaction = await shoppingOperations.FetchFlightOffersPricingAsync(offerId, officeId, amaClientRef, includedFareRules);

            // If transaction failed...
            if (!transaction.Successful)
            {
                // Return the problem
                return Problem(
                    detail: transaction.ErrorMessage,
                    statusCode: transaction.StatusCode);
            }

            return Ok(transaction.Result);
        }
    }
}
