namespace API.DTOS
{
    public class BasketDto
    {
        public int Id{get;set;}
        public string BuyerId{get;set;}
        public List<BsketItemDto> Items{get;set;}
    }
}