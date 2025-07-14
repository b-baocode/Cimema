using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface ICommentRatingService
    {
        Task<PagedResult<CommentRatingResponse>> GetByMovieIdAsync(int movieId, CommentRatingPagingRequest request);
        Task<CommentRatingResponse> CreateAsync(CommentRatingRequest request);

        // Xóa cần chuyền RoleId để cấp quyền xóa
        Task<bool> DeleteAsync(int id, string userId, string userRole);
    }
}