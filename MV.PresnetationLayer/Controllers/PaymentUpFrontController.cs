using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.ServiceInterfaces;
using System;
using System.Threading.Tasks;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentUpFrontController : ControllerBase
    {
        private readonly IPaymentUpFrontService _paymentUpFrontService;

        public PaymentUpFrontController(IPaymentUpFrontService paymentUpFrontService)
        {
            _paymentUpFrontService = paymentUpFrontService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> CreatePaymentUpFront([FromBody] PaymentUpFrontRequest paymentRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdPayment = await _paymentUpFrontService.CreatePaymentUpFrontAsync(paymentRequest);
                return CreatedAtAction(nameof(GetPaymentUpFrontById), new { id = createdPayment.PaymentUpFrontId }, createdPayment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetPaymentUpFrontById(int id)
        {
            var payment = await _paymentUpFrontService.GetPaymentUpFrontByIdAsync(id);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found" });
            }
            return Ok(payment);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetAllPaymentUpFronts()
        {
            var payments = await _paymentUpFrontService.GetAllPaymentUpFrontsAsync();
            return Ok(payments);
        }

        /*     [HttpPut("{id}")]
             [Authorize(Roles = "Admin,Manager,Employee")]
             public async Task<IActionResult> UpdatePaymentUpFront(int id, [FromBody] PaymentUpFrontRequest paymentRequest)
             {
                 if (!ModelState.IsValid)
                 {
                     return BadRequest(ModelState);
                 }

                 try
                 {
                     var updatedPayment = await _paymentUpFrontService.UpdatePaymentUpFrontAsync(id, paymentRequest);
                     if (updatedPayment == null)
                     {
                         return NotFound(new { message = "Payment not found" });
                     }
                     return Ok(updatedPayment);
                 }
                 catch (Exception ex)
                 {
                     return BadRequest(new { message = ex.Message });
                 }
             }

             [HttpDelete("{id}")]
             [Authorize(Roles = "Admin,Manager,Employee")]
             public async Task<IActionResult> DeletePaymentUpFront(int id)
             {
                 var result = await _paymentUpFrontService.DeletePaymentUpFrontAsync(id);
                 if (!result)
                 {
                     return NotFound(new { message = "Payment not found" });
                 }
                 return NoContent();
             }*/
    }
}