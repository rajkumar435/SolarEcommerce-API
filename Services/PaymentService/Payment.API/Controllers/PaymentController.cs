
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Infrastructure.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IRazorpayService _razorpayService;
        private readonly ILogger<PaymentController> _logger;
        private readonly IOrderClient _orderClient;
        private readonly IPaymentProcessingService _paymentProcessingService;
        public PaymentController(
            IPaymentService paymentService,
            IRazorpayService razorpayService,
            IPaymentProcessingService paymentProcessingService,
            ILogger<PaymentController> logger,  IOrderClient orderClient)
        {
            _paymentService = paymentService;
            _razorpayService = razorpayService;
            _logger = logger;
            _paymentProcessingService =
        paymentProcessingService;

            _orderClient = orderClient;
        }

        // =========================================================
        // CREATE PAYMENT RECORD
        // POST: api/payments
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> CreatePayment(
            [FromBody] CreatePaymentRequest request)
        {
            try
            {
                // -------------------------------------------------
                // STEP 1: Check request
                // -------------------------------------------------

                if (request == null)
                {
                    _logger.LogWarning(
                        "CreatePayment failed because request body is null.");

                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Payment request cannot be null."
                    });
                }

                // -------------------------------------------------
                // STEP 2: Validate ModelState
                // -------------------------------------------------

                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState
                        .Where(x => x.Value != null &&
                                    x.Value.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value.Errors
                                .Select(e =>
                                    string.IsNullOrWhiteSpace(e.ErrorMessage)
                                        ? "Invalid value."
                                        : e.ErrorMessage)
                                .ToArray());

                    _logger.LogWarning(
                        "CreatePayment model validation failed. Errors: {@Errors}",
                        validationErrors);

                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid payment request.",
                        Errors = validationErrors
                    });
                }

                // -------------------------------------------------
                // STEP 3: Validate Amount
                // -------------------------------------------------

                if (request.Amount <= 0)
                {
                    _logger.LogWarning(
                        "Invalid payment amount. Amount: {Amount}",
                        request.Amount);

                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Payment amount must be greater than zero."
                    });
                }

                // -------------------------------------------------
                // STEP 4: Validate OrderId
                // -------------------------------------------------

                if (request.OrderId <= 0)
                {
                    _logger.LogWarning(
                        "Invalid OrderId. OrderId: {OrderId}",
                        request.OrderId);

                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Valid OrderId is required."
                    });
                }

                // -------------------------------------------------
                // STEP 5: Validate UserId
                // -------------------------------------------------

                if (request.UserId <= 0)
                {
                    _logger.LogWarning(
                        "Invalid UserId. UserId: {UserId}",
                        request.UserId);

                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Valid UserId is required."
                    });
                }

                // -------------------------------------------------
                // STEP 6: Validate Currency
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(request.Currency))
                {
                    _logger.LogWarning(
                        "Currency is missing. OrderId: {OrderId}",
                        request.OrderId);

                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Currency is required."
                    });
                }

                // -------------------------------------------------
                // STEP 7: Validate PaymentMethod
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                {
                    _logger.LogWarning(
                        "PaymentMethod is missing. OrderId: {OrderId}",
                        request.OrderId);

                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Payment method is required."
                    });
                }

                // -------------------------------------------------
                // STEP 8: Log request
                // -------------------------------------------------

                _logger.LogInformation(
                    "Payment creation started. OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}, Currency: {Currency}, PaymentMethod: {PaymentMethod}",
                    request.OrderId,
                    request.UserId,
                    request.Amount,
                    request.Currency,
                    request.PaymentMethod);

                // -------------------------------------------------
                // STEP 9: Create internal payment record
                // -------------------------------------------------

                var result =
                    await _paymentService.CreatePayment(request);

                // -------------------------------------------------
                // STEP 10: Validate service result
                // -------------------------------------------------

                if (result == null)
                {
                    _logger.LogError(
                        "Payment service returned null. OrderId: {OrderId}",
                        request.OrderId);

                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new
                        {
                            Success = false,
                            Message =
                                "Payment service returned an empty response."
                        });
                }

                // -------------------------------------------------
                // STEP 11: Return result
                // -------------------------------------------------

                _logger.LogInformation(
                    "Payment record created successfully. OrderId: {OrderId}",
                    request.OrderId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Invalid payment request. OrderId: {OrderId}",
                    request == null ? 0 : request.OrderId);

                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Payment business validation failed. OrderId: {OrderId}",
                    request == null ? 0 : request.OrderId);

                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while creating payment. OrderId: {OrderId}",
                    request == null ? 0 : request.OrderId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Success = false,
                        Message =
                            "An unexpected error occurred while processing the payment."
                    });
            }
        }

        // =========================================================
        // CREATE RAZORPAY ORDER
        // POST: api/payments/razorpay/order
        // =========================================================

        [HttpPost("razorpay/order")]
        public async Task<IActionResult> CreateRazorpayOrder(
      [FromBody] CreateRazorpayOrderRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Razorpay order request cannot be null."
                    });
                }

                if (request.OrderId <= 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Valid OrderId is required."
                    });
                }

                if (request.UserId <= 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Valid UserId is required."
                    });
                }

                if (request.Amount <= 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Amount must be greater than zero."
                    });
                }

                var result =
                    await _razorpayService.CreateOrder(request);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Invalid Razorpay order request.");

                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Razorpay order creation failed.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Success = false,
                        Message =
                            "Unable to create Razorpay order."
                    });
            }
        }
        // =========================================================
        // VERIFY RAZORPAY PAYMENT
        // POST: api/payments/razorpay/verify
        // =========================================================

        [HttpPost("razorpay/verify")]
        public async Task<IActionResult> VerifyRazorpayPayment(
       [FromBody] VerifyPaymentRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Payment verification request cannot be null."
                    });
                }

                if (request.OrderId <= 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Valid OrderId is required."
                    });
                }

                if (request.UserId <= 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Valid UserId is required."
                    });
                }

                if (request.Amount <= 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Amount must be greater than zero."
                    });
                }

                if (string.IsNullOrWhiteSpace(
                    request.Currency))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Currency is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(
                    request.RazorpayOrderId))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "RazorpayOrderId is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(
                    request.RazorpayPaymentId))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "RazorpayPaymentId is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(
                    request.RazorpaySignature))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "RazorpaySignature is required."
                    });
                }

                // -------------------------------------------------
                // STEP 1
                // Verify Razorpay signature
                // -------------------------------------------------

                _logger.LogInformation(
                    "Verifying Razorpay payment. " +
                    "OrderId: {OrderId}",
                    request.OrderId);

                var signatureValid =
                    _razorpayService.VerifyPayment(
                        request);

                if (!signatureValid)
                {
                    _logger.LogWarning(
                        "Invalid Razorpay signature. " +
                        "OrderId: {OrderId}",
                        request.OrderId);

                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Payment signature verification failed."
                    });
                }

                // -------------------------------------------------
                // STEP 2
                // Record payment + update order
                // -------------------------------------------------

                var processed =
                    await _paymentProcessingService
                        .ProcessSuccessfulRazorpayPayment(
                            request);

                if (!processed)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new
                        {
                            Success = false,
                            Message =
                                "Payment verified but order update failed."
                        });
                }

                // -------------------------------------------------
                // STEP 3
                // Success
                // -------------------------------------------------

                _logger.LogInformation(
                    "Razorpay payment processed successfully. " +
                    "OrderId: {OrderId}",
                    request.OrderId);

                return Ok(new
                {
                    Success = true,

                    OrderId =
                        request.OrderId,

                    UserId =
                        request.UserId,

                    Amount =
                        request.Amount,

                    Currency =
                        request.Currency,

                    RazorpayOrderId =
                        request.RazorpayOrderId,

                    RazorpayPaymentId =
                        request.RazorpayPaymentId,

                    Message =
                        "Payment verified and order updated successfully."
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Invalid Razorpay verification request.");

                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected Razorpay verification error.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Success = false,
                        Message =
                            "Payment verification failed."
                    });
            }
        }
        // =========================================================
        // GET PAYMENT BY ID
        // GET: api/payments/{id}
        // =========================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Valid payment ID is required."
                    });
                }

                var payment =
                    await _paymentService.GetPaymentById(id);

                if (payment == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Payment not found."
                    });
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while getting payment. PaymentId: {PaymentId}",
                    id);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Success = false,
                        Message =
                            "An error occurred while getting the payment."
                    });
            }
        }

        // =========================================================
        // GET PAYMENT BY ORDER ID
        // GET: api/payments/order/{orderId}
        // =========================================================

        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Valid OrderId is required."
                    });
                }

                var payment =
                    await _paymentService
                        .GetPaymentByOrderId(orderId);

                if (payment == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message =
                            "Payment not found for the given order."
                    });
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while getting payment by OrderId. OrderId: {OrderId}",
                    orderId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Success = false,
                        Message =
                            "An error occurred while getting the payment."
                    });
            }
        }
    }
}

