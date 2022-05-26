using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Entities.eShopDapr
{
    public class ApplicationUser
    {
        [Required]
        public string CardNumber { get; set; }

        [Required]
        public string SecurityNumber { get; set; }
        
        [Required]
        public DateTime CardExpiration { get; set; }

        [Required]
        public string CardHolderName { get; set; }

        public int CardType { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string ZipCode { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string LastName { get; set; }

    }

}
