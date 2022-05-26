namespace Common.Entities.eShopDapr
{
   
    public class CatalogItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string PictureFileName { get; set; }

        public int CatalogTypeId { get; set; }

        public CatalogType CatalogType { get; set; } = null!;

        public int CatalogBrandId { get; set; }

        public CatalogBrand CatalogBrand { get; set; } = null!;

        public int AvailableStock { get; set; }


    }
}
