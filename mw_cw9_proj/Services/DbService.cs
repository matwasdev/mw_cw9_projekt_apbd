using System.Data;
using Microsoft.Data.SqlClient;
using mw_cw9_proj.Exceptions;
using mw_cw9_proj.Models.DTOs;

namespace mw_cw9_proj.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;

    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? String.Empty;
    }
    
    
    public async Task<int> CreateProductWarehouseAsync(CreateProductWarehouseDTO createProductWarehouseDTO)
    {
        string checkQuery =
            @"SELECT 1 WHERE Exists(SELECT 1 FROM PRODUCT WHERE IdProduct=@IdProduct) AND Exists(SELECT 1 FROM WAREHOUSE WHERE IdWarehouse=@IdWarehouse)";
        

        using (var sqlConnection = new SqlConnection(_connectionString))
        {
            int idOrder = 0;
            decimal price = 0;


            await sqlConnection.OpenAsync();

            using (var checkCommand = new SqlCommand(checkQuery, sqlConnection))
            {
                checkCommand.Parameters.AddWithValue("@IdProduct", createProductWarehouseDTO.IdProduct);
                checkCommand.Parameters.AddWithValue("@IdWarehouse", createProductWarehouseDTO.IdWarehouse);

                if (createProductWarehouseDTO.Amount <= 0)
                    throw new BadRequestException("Amount must be greater than 0");

                using (var reader = await checkCommand.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        throw new NotFoundException("Product or Warehouse not found");
                    }
                }
            }


            using (var checkOrderCmd =
                   new SqlCommand(
                       "SELECT IdOrder FROM \"ORDER\" WHERE IdProduct = @IdProduct and CreatedAt < @CreatedAt and Amount = @Amount;",
                       sqlConnection))
            {
                checkOrderCmd.Parameters.AddWithValue("@IdProduct", createProductWarehouseDTO.IdProduct);
                checkOrderCmd.Parameters.AddWithValue("@CreatedAt", createProductWarehouseDTO.CreatedAt);
                checkOrderCmd.Parameters.AddWithValue("@Amount", createProductWarehouseDTO.Amount);

                using (var reader = await checkOrderCmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        throw new NotFoundException("A proper order not found");
                    }

                    idOrder = reader.GetInt32(0);
                }
            }


            using (var checkRealizationCmd =
                   new SqlCommand("SELECT 1 FROM Product_Warehouse where IdOrder = @IdOrder", sqlConnection))
            {
                checkRealizationCmd.Parameters.AddWithValue("@IdOrder", idOrder);

                using (var reader = await checkRealizationCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        throw new ConflictException("This order was already realised");
                    }
                }
            }



            using (var getProdCmd =
                   new SqlCommand("SELECT Price FROM Product where IdProduct=@IdProduct", sqlConnection))
            {
                getProdCmd.Parameters.AddWithValue("@IdProduct", createProductWarehouseDTO.IdProduct);

                using (var reader = await getProdCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        price = reader.GetDecimal(0);
                    }
                }
            }


            //BEGIN TRANSACTION
            using (var transaction = await sqlConnection.BeginTransactionAsync())
            {
                try
                {
                    using (var updateFullfiledAtCmd =
                           new SqlCommand("UPDATE \"ORDER\" SET FulfilledAt = GETDATE() where IdOrder=@IdOrder",
                               sqlConnection))
                    {
                        updateFullfiledAtCmd.Transaction = transaction as SqlTransaction;
                        updateFullfiledAtCmd.Parameters.AddWithValue("@IdOrder", idOrder);

                        await updateFullfiledAtCmd.ExecuteNonQueryAsync();
                    }

                    int generatedId = 0;
                    using (var createCmd =
                           new SqlCommand(
                               "INSERT INTO Product_Warehouse(IdWarehouse,IdProduct,IdOrder,Amount,Price,CreatedAt) values (@IdWarehouse,@IdProduct,@IdOrder,@Amount,@Price,GETDATE()); SELECT SCOPE_IDENTITY();",
                               sqlConnection))
                    {
                        createCmd.Transaction = transaction as SqlTransaction;

                        createCmd.Parameters.AddWithValue("IdWarehouse", createProductWarehouseDTO.IdWarehouse);
                        createCmd.Parameters.AddWithValue("IdProduct", createProductWarehouseDTO.IdProduct);
                        createCmd.Parameters.AddWithValue("IdOrder", idOrder);
                        createCmd.Parameters.AddWithValue("Amount", createProductWarehouseDTO.Amount);
                        createCmd.Parameters.AddWithValue("Price", price * createProductWarehouseDTO.Amount);

                        var res = await createCmd.ExecuteScalarAsync();
                        generatedId = Convert.ToInt32(res);
                    }

                    await transaction.CommitAsync();
                    return generatedId;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }


    public async Task<int> CreateProductWarehouseWithProcedureAsync(CreateProductWarehouseDTO createProductWarehouseDTO)
    {
        using (var sqlConnection = new SqlConnection(_connectionString))
        {
            int generatedId = 0;
            await sqlConnection.OpenAsync();

            using (var cmd = new SqlCommand("AddProductToWarehouse", sqlConnection))
            {
                cmd.Parameters.AddWithValue("@IdWarehouse", createProductWarehouseDTO.IdWarehouse);
                cmd.Parameters.AddWithValue("@CreatedAt", createProductWarehouseDTO.CreatedAt);
                cmd.Parameters.AddWithValue("@Amount", createProductWarehouseDTO.Amount);
                cmd.Parameters.AddWithValue("@IdProduct", createProductWarehouseDTO.IdProduct);
                
                cmd.CommandType = CommandType.StoredProcedure;

                var res= await cmd.ExecuteScalarAsync();
                generatedId = Convert.ToInt32(res);
            }
            return generatedId;
        }
    }


}