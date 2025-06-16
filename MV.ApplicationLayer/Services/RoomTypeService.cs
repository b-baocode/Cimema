using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.GenericExceptionReport;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.SpecificExceptionReport;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.Services
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseStorageService _firebaseStorageService;

        public RoomTypeService(IUnitOfWork unitOfWork, IFirebaseStorageService firebaseStorageService)
        {
            _unitOfWork = unitOfWork;
            _firebaseStorageService = firebaseStorageService;
        }

        public async Task<bool> CheckTypeExistByIdAsync(int roomTypeId)
        {
            var checkResult = await _unitOfWork.roomTypeRepository.CheckTypeExistAsync(roomTypeId);

            return checkResult;
        }

        public async Task<PagedResult<GetAllRoomTypeWithRoomAdminResponse>> GetAllRoomTypeAdminAsync(GetAllRoomTypeAdminRequest getAllRoomTypeAdminRequest)
        {
            var roomTypes = await _unitOfWork.roomTypeRepository.GetAllRoomTypeWithRoomAsync();

            var totalItems = await _unitOfWork.roomTypeRepository.GetTotalRoomTypeCountAsync();

            var response = roomTypes.Select(
                rt => new GetAllRoomTypeWithRoomAdminResponse
                {
                    RoomTypeId = rt.RoomTypeId,
                    RoomTypeName = rt.RoomTypeName,
                    RoomTypePrice = rt.RoomTypePrice,
                    TypeDescription = rt.TypeDescription,
                    RoomTypePicture = rt.RoomTypePicture,
                    RoomTypeStatus = rt.RoomTypeStatus,
                    RoomsUsedRoomType = rt.RoomsUsedRoomType?.Select(
                        r => new RoomForRoomType
                        {
                            RoomId = r.RoomId,
                            RoomName = r.RoomName,
                            RoomStatus = r.RoomStatus,
                        }).ToList()
                });

            return new PagedResult<GetAllRoomTypeWithRoomAdminResponse>
            {
                Items = response.ToList(),
                TotalItems = totalItems,
                Page = getAllRoomTypeAdminRequest.Page,
                PageSize = getAllRoomTypeAdminRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)getAllRoomTypeAdminRequest.PageSize)
            };
        }

        public async Task<RoomTypeCreateResponse> CreateRoomTypeAsync(RoomTypeCreateRequest roomTypeCreateRequest, Stream imageStream, string imageName)
        {
            string imgUrl;

            imgUrl = await _firebaseStorageService.UploadImageAsync(imageStream, imageName, "RoomTypeImages");

            if (imgUrl == null)
            {
                return null;
            }


            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

            var roomTypeName = textInfo.ToTitleCase(roomTypeCreateRequest.RoomTypeName.ToLower());

            var roomTypeCreate = new RoomType
            {
                RoomTypeName = roomTypeName,
                RoomTypePrice = roomTypeCreateRequest.RoomTypePrice,
                TypeDescription = roomTypeCreateRequest.TypeDescription,
                RoomTypePicture = imgUrl,
                Status = "Active"
            };

            await _unitOfWork.roomTypeRepository.AddAsync(roomTypeCreate);

            int affectedRow;

            try
            {
                affectedRow = await _unitOfWork.SaveChangesAsync();

            }
            catch (UniqueConstraintViolationException ex)
            {
                await _firebaseStorageService.DeleteImageAsync(imgUrl);

                throw new RoomTypeNameAlreadyExistException(roomTypeCreate.RoomTypeName, "This room type name already in used", ex);
            }
            catch (Exception ex)
            {
                await _firebaseStorageService.DeleteImageAsync(imgUrl);

                throw;
            }

            var response = new RoomTypeCreateResponse
            {
                RoomTypeId = roomTypeCreate.RoomTypeId,
                RoomTypeName = roomTypeCreate.RoomTypeName,
                TypeDescription = roomTypeCreate.TypeDescription,
                RoomTypePrice = roomTypeCreate.RoomTypePrice,
                RoomTypePictureURL = imgUrl,
                RoomTypeStatus = roomTypeCreate.Status,
            };

            return response;
        }

    }
}
