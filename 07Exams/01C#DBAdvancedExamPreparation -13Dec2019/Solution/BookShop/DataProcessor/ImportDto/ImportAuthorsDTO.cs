using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookShop.DataProcessor.ImportDto
{
    public class ImportAuthorsDTO
    {
        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        public string FirstName { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [RegularExpression("^[0-9]{3}-[0-9]{3}-[0-9]{4}$")]
        public string Phone { get; set; }
        public ICollection<ImportAutohorsBooksDTO> Books { get; set; }
    }

    public class ImportAutohorsBooksDTO
    {
        public int?  Id { get; set; }
    }
}
