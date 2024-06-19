using API.Entities.OrderAggregate;

namespace API.DTOS
{
    public class CreateOrderDto
    {
        public bool SaveAddress{ get; set; }
        public ShippingAddress ShipToAddress {get; set;}
        
    }
}