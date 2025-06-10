using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.SpecificExceptionReport;
using MV.ApplicationLayer.ServiceInterfaces;
using Microsoft.OpenApi.Services;

namespace MV.PresnetationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize (Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }


        //Create new room and add seats automatically, all seats are standard
        [HttpPost("CreateRoom")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RoomCreateResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<RoomCreateResponse>> RoomCreate([FromBody] RoomCreateRequest roomCreateRequest)
        {
            try
            {
                var createdRoom = await _roomService.AddRoomWithSeatsAsync(roomCreateRequest);

                return CreatedAtAction(
                    nameof(GetRoomById),
                    new { id = createdRoom.RoomId },
                    createdRoom
                    );
            }
            catch (RoomNameAlreadyExistsException ex)
            {
                return Conflict(
                    new ProblemDetails
                    {
                        Title = "Room name already exists",
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



        //Get room with ID that returns room with seats data
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetRoomWithSeatsByIdResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<GetRoomWithSeatsByIdResponse>> GetRoomById(int id)
        {

            try
            {
                var searchedResult = await _roomService.GetRoomWithSeatsAsync(id);

                if (searchedResult == null)
                {
                    return NotFound(
                        new ProblemDetails
                        {
                            Title = "Room not found",
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


        //Delete room
        [HttpDelete("{deleteRoomId}/DeleteRoom")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult> DeleteRoomById(int deleteRoomId)
        {
            try
            {
                var deleteResult = await _roomService.DeleteRoomAsync(deleteRoomId);

                if (!deleteResult)
                {
                    return NotFound(
                        new ProblemDetails
                        {
                            Title = "Room not found",
                            Status = StatusCodes.Status404NotFound,
                            Detail = $"Room with ID {deleteRoomId} does not exist.",
                            Instance = HttpContext.Request.Path
                        }
                        );
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


        //Undelete room
        [HttpPatch("{unDeleteRoomId}/UnDeleteRoom")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult> UnDeleteRoomById(int unDeleteRoomId)
        {
            try
            {
                var unDeleteResult = await _roomService.UnDeleteRoomAsync(unDeleteRoomId);

                if (!unDeleteResult)
                {
                    return NotFound(
                        new ProblemDetails
                        {
                            Title = "Room not found",
                            Status = StatusCodes.Status404NotFound,
                            Detail = $"Room with ID {unDeleteRoomId} does not exist.",
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


        //Get all room
        [HttpGet("GetAllRoom")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<GetAllRoomResponse>))]
        public async Task<ActionResult<PagedResult<GetAllRoomResponse>>> GetAllCinemaroom([FromQuery] GetAllRoomRequest getAllRoomRequest)
        {
            try
            {
                var result = await _roomService.GetAllRoomAsync(getAllRoomRequest);

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
    }
}
