using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.SpecificExceptionReport;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize (Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public class RoomTypeController : ControllerBase
    {
        private readonly IRoomTypeService _roomTypeService;

        public RoomTypeController(IRoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        [HttpGet("GetRoomTypeAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<GetAllRoomTypeWithRoomAdminResponse>))]
        public async Task<ActionResult<PagedResult<GetAllRoomTypeWithRoomAdminResponse>>> GetAllRoomTypeWithRoomAdmin([FromQuery] GetAllRoomTypeAdminRequest getAllRoomTypeAdminRequest)
        {
            try
            {
                var result = await _roomTypeService.GetAllRoomTypeAdminAsync(getAllRoomTypeAdminRequest);

                return Ok(result);

            }
            catch (Exception ex)
            {
                return Problem(
                    detail: "An unexpected error occurred while getting all the room. Please try again later.",
                    title: "Internal Server Error",
                    statusCode: StatusCodes.Status500InternalServerError,
                    instance: HttpContext.Request.Path
                );
            }
        }



        [HttpPost("CreateRoomType")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RoomTypeCreateResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<RoomTypeCreateResponse>> CreateRoomtType([FromForm] RoomTypeCreateRequest roomTypeCreateRequest, IFormFile roomTypeImage)
        {
            if (roomTypeImage == null || roomTypeImage.Length == 0)
            {
                return BadRequest("Image is required");
            }

            if (roomTypeImage.ContentType != "image/jpeg" && roomTypeImage.ContentType != "image/jpg")
            {
                return BadRequest("Only JPEG or JPG images are allowed.");
            }

            Stream imgStream = roomTypeImage.OpenReadStream();
            string imgName = roomTypeImage.FileName;

            try
            {
                var createRoomType = await _roomTypeService.CreateRoomTypeAsync(roomTypeCreateRequest, imgStream, imgName);

                if (createRoomType == null)
                {
                    return BadRequest("Image url is null");
                }

                return CreatedAtAction(
                    nameof(GetRoomTypeById),
                    new { id = createRoomType.RoomTypeId },
                    createRoomType
                    );
            }
            catch (RoomTypeNameAlreadyExistException ex)
            {
                return Conflict(
                    new ProblemDetails
                    {
                        Title = "Room type name already exists",
                        Status = StatusCodes.Status409Conflict,
                        Detail = ex.Message,
                        Instance = HttpContext.Request.Path,
                        Extensions =
                        {
                            {"ConflictingName", ex.ConflictingName}
                        }
                    });
            }
            catch (Exception ex)
            {
                return Problem(
                    detail: "An unexpected error occurred while creating the room. Please try again later.",
                    title: "Internal Server Error",
                    statusCode: StatusCodes.Status500InternalServerError,
                    instance: HttpContext.Request.Path
                );
            }

        }



        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetRoomWithSeatsByIdResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<GetAllRoomTypeWithRoomAdminResponse>> GetRoomTypeById([FromRoute] int id)
        {
            try
            {
                var searchedResult = await _roomTypeService.GetRoomTypeByIdWithRoomAsync(id);

                if (searchedResult == null)
                {
                    return NotFound(
                        new ProblemDetails
                        {
                            Title = "Room Type not found",
                            Status = StatusCodes.Status404NotFound,
                            Detail = $"Room with ID {id} does not exist.",
                            Instance = HttpContext.Request.Path
                        }
                        );
                }

                return Ok(searchedResult);

            }
            catch (Exception ex)
            {
                return Problem(
                    detail: "An unexpected error occurred while searching the room. Please try again later.",
                    title: "Internal Server Error",
                    statusCode: StatusCodes.Status500InternalServerError,
                    instance: HttpContext.Request.Path
                );
            }
        }


        [HttpDelete("DeleteRoomType/{DeleteRoomTypeId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        public async Task<ActionResult> DeleteRoomTypeById([FromRoute] int DeleteRoomTypeId)
        {
            try
            {
                var (deleteResult, errorMessage) = await _roomTypeService.DeleteRoomTypeAsync(DeleteRoomTypeId);

                if (!deleteResult)
                {
                    if (errorMessage == "Not exist")
                    {
                        return NotFound(
                        new ProblemDetails
                        {
                            Title = "Room not found",
                            Status = StatusCodes.Status404NotFound,
                            Detail = $"Room type with ID {DeleteRoomTypeId} does not exist.",
                            Instance = HttpContext.Request.Path
                        });
                    }
                    else
                    {
                        return BadRequest(
                        new ProblemDetails
                        {
                            Title = $"{errorMessage}",
                            Status = StatusCodes.Status400BadRequest,
                            Instance = HttpContext.Request.Path
                        });
                    }
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(
                   detail: "An unexpected error occurred while deleting the room. Please try again later.",
                   title: "Internal Server Error",
                   statusCode: StatusCodes.Status500InternalServerError,
                   instance: HttpContext.Request.Path
               );
            }
        }


        [HttpPatch("UnDeleteRoom/{UnDeleteRoomId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult> UnDeleteRoomById([FromRoute] int UnDeleteRoomId)
        {
            try
            {
                var unDeleteResult = await _roomTypeService.UnDeleteRoomTypeAsync(UnDeleteRoomId);

                if (!unDeleteResult)
                {
                    return NotFound(
                        new ProblemDetails
                        {
                            Title = "Room not found",
                            Status = StatusCodes.Status404NotFound,
                            Detail = $"Room with ID {UnDeleteRoomId} does not exist.",
                            Instance = HttpContext.Request.Path
                        }
                        );
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(
                   detail: "An unexpected error occurred while undeleting the room. Please try again later.",
                   title: "Internal Server Error",
                   statusCode: StatusCodes.Status500InternalServerError,
                   instance: HttpContext.Request.Path
               );
            }
        }
    }
}
