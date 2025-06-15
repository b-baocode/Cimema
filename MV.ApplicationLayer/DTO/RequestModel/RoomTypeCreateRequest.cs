using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class RoomTypeCreateRequest
    {
        [Required(ErrorMessage = "Room type name is required")]
        [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string RoomTypeName { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(typeof(decimal), "0", "9999999999999999.99", ErrorMessage = "Price must be >= 0 and less than 9999999999999999.99")]
        public decimal RoomTypePrice { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string TypeDescription { get; set; }

        
    }
}
