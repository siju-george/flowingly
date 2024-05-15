using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Helpers;
using WebApi.Helpers.Exceptions;
using WebApi.Helpers.Pagination;
using WebApi.Services.DataTransferObjects.UserService;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

/// <summary>
/// The controller for handling user related requests.
/// <c>CreatedAtAction</c> will not work if you use "Async" suffix in controller action name.
/// You must specify <c>ActionName</c> attribute to fix this known issue.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthHelper _authHelper;

    /// <summary>
    /// extract total from text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("ExtractData")]
    public IActionResult ExtractData([FromBody] string text)
    {
        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml($"<root>{text}</root>");
            var expenseNode = xmlDoc.SelectSingleNode("//expense");
            if (expenseNode == null)
            {
                return BadRequest("No expense node found");
            }
            var costCentreNode = expenseNode.SelectSingleNode("cost_centre");
            var costCentre = costCentreNode?.InnerText ?? "UNKNOWN";
            var totalNode = expenseNode.SelectSingleNode("total");
            if (totalNode == null)
            {
                return BadRequest("No total node found");
            }
            var total = decimal.Parse(totalNode.InnerText.Replace(",", ""));
            var taxRate = 0.2m; // assume 20% tax rate
            var tax = total * taxRate;
            var totalExcludingTax = total - tax;
            return Ok(new
            {
                CostCentre = costCentre,
                Total = total,
                Tax = tax,
                TotalExcludingTax = totalExcludingTax
            });
        }
        catch (XmlException ex)
        {
            return BadRequest("Invalid XML format: " + ex.Message);
        }
    }



}

