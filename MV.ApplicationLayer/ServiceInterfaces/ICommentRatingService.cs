using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface ICommentRatingService
    {
        Task<PagedResult<CommentRatingResponse>> GetByMovieIdAsync(int movieId, CommentRatingPagingRequest request);
        Task<CommentRatingResponse> CreateAsync(CommentRatingRequest request);
        Task<bool> DeleteAsync(int id);
    }
} 