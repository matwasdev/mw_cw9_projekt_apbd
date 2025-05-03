using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using mw_cw9_proj.Exceptions;
using mw_cw9_proj.Models.DTOs;
using mw_cw9_proj.Services;

namespace mw_cw9_proj.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehousesController : ControllerBase
{

    private readonly IWarehousesService _warehousesService;

    public WarehousesController(IWarehousesService warehousesService)
    {
        _warehousesService = warehousesService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateProductWarehouse(CreateProductWarehouseDTO createProductWarehouseDTO)
    {
        try
        {
            int generatedId = await _warehousesService.CreateProductWarehouse(createProductWarehouseDTO);
            return Ok(generatedId);
        }
        catch (InvalidAmountException)
        {
            return BadRequest("Invalid amount (cannot be less or equal 0)");
        }
        catch (InvalidIdException)
        {
            return NotFound("IdProduct or IdWarehouse could not be found");
        }
        catch (OrderNotFoundException)
        {
            return NotFound("A proper order to realize could not be found");
        }
        catch (OrderAlreadyRealizedException)
        {
            return BadRequest("This order is already realized");
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal Server Error occured");
        }
    }

    [HttpPost("procedure")]
    public async Task<ActionResult> CreateProductWarehouseWithProcedure(CreateProductWarehouseDTO createProductWarehouseDTO)
    {
        try
        {
            int generatedId = await _warehousesService.CreateProductWarehouseWithProcedure(createProductWarehouseDTO);
            return Ok(generatedId);
        }
        catch (SqlException e)
        {
            Console.WriteLine(e.Message);
            if (e.Message.Contains("IdProduct"))
                return NotFound("IdProduct could not be found");

            if (e.Message.Contains("IdWarehouse"))
                return NotFound("IdWarehouse could not be found");

            if (e.Message.Contains("no order"))
                return NotFound("A proper order could not be found");

            return StatusCode(500, "Database error occurred");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return StatusCode(500, "Internal Server Error occured");
        }
    }





}