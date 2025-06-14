using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class RoomUpdateRequest
    {
        //[Required]
        //public int RoomId { get; set; }

        [Required(ErrorMessage = "Room name is required")]
        [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Range(1, 10, ErrorMessage = "Rows must be between 1 and 10.")]
        public int Rows { get; set; }

        [Range(1, 10, ErrorMessage = "Columns must be between 1 and 10.")]
        public int Columns { get; set; }

        [Required]
        public int RoomTypeId { get; set; }
    }
}
