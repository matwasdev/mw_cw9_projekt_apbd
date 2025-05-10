using mw_cw9_proj.Models.DTOs;

namespace mw_cw9_proj.Services;

public interface IDbService
{
    Task<int> CreateProductWarehouseAsync(CreateProductWarehouseDTO createProductWarehouseDTO);
    Task<int> CreateProductWarehouseWithProcedureAsync(CreateProductWarehouseDTO createProductWarehouseDTO);
}