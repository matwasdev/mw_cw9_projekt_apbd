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

    private readonly IDbService _dbService;

    public WarehousesController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateProductWarehouse(CreateProductWarehouseDTO createProductWarehouseDTO)
    {
        try
        {
            int generatedId = await _dbService.CreateProductWarehouseAsync(createProductWarehouseDTO);
            return Ok(generatedId);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (BadRequestException e)
        {
            return BadRequest(e.Message);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "Internal Server Error occured");
        }
    }

    [HttpPost("procedure")]
    public async Task<ActionResult> CreateProductWarehouseWithProcedure(CreateProductWarehouseDTO createProductWarehouseDTO)
    {
        try
        {
            int generatedId = await _dbService.CreateProductWarehouseWithProcedureAsync(createProductWarehouseDTO);
            return Ok(generatedId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            if (e.Message.Contains("IdProduct"))
                return NotFound("IdProduct could not be found");

            if (e.Message.Contains("IdWarehouse"))
                return NotFound("IdWarehouse could not be found");
            
            if (e.Message.Contains("no order to fulfill"))
                return BadRequest("No order to fulfill");


            return StatusCode(500, "Database error occurred");
        }
    }





}