namespace mw_cw9_proj.Models.DTOs;

public class CreateProductWarehouseDTO
{
   public int IdProduct { get; set; }
   public int IdWarehouse { get; set; }
   public int Amount { get; set; }
   public DateTime CreatedAt { get; set; }
}