namespace Bags_Shop_API.Services.ProductServices.ProductsDtos
{
	public record CreateProductResponseDto 
	{
        public int Id { get; set; }
       

        public string ArName { get; set; }

        public string EnName { get; set; }

        public string ArDescription { get; set; }

        public string EnDescription { get; set; }

       
        public int Quantity { get; set; }

    }

}
