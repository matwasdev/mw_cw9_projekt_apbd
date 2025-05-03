using mw_cw9_proj.Models.DTOs;

namespace mw_cw9_proj.Services;

public interface IWarehousesService
{
    Task<int> CreateProductWarehouse(CreateProductWarehouseDTO createProductWarehouseDTO);
    Task<int> CreateProductWarehouseWithProcedure(CreateProductWarehouseDTO createProductWarehouseDTO);
}