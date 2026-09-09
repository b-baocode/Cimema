using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class RoomTypePriceUpdateRequest
    {
        [Required(ErrorMessage = "Room type Id is required")]
        public int RoomTypeId { get; set; }

        //[Required(ErrorMessage = "Price is required")]
        [Range(typeof(decimal), "0", "9999999999999999.99", ErrorMessage = "Price must be >= 0 and less than 9999999999999999.99")]
        public decimal? RoomTypePriceUpdate { get; set; }
    }
}
