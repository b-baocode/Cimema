using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using System;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.Services
{
    public class ScoreService : IScoreService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ScoreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        /// <summary>
        /// Tạo mới bản ghi điểm cho user (dùng khi user mới đăng ký)
        /// </summary>
        public async Task AddScoreAsync(Score score)
        {
            await _unitOfWork.scoreRepository.AddAsync(score);
        }
        /// <summary>
        /// Cập nhật điểm cho user (dùng cho các trường hợp đặc biệt)
        /// </summary>
        public async Task UpdateScoreAsync(Score score)
        {
            await _unitOfWork.scoreRepository.UpdateAsync(score);
        }
        /// <summary>
        /// Lưu lịch sử điểm (dùng cho các thao tác cộng/trừ điểm thủ công)
        /// </summary>
        public async Task AddScoreHistoryAsync(ScoreHistory scoreHistory)
        {
            await _unitOfWork.scoreHistoryRepository.AddAsync(scoreHistory);
        }
        /// <summary>
        /// Cộng điểm cho user khi thanh toán hóa đơn thành công (1% tổng tiền, lưu lịch sử)
        /// </summary>
        public async Task AddScoreForInvoiceAsync(string userId, int invoiceId, decimal totalPrice)
        {
            int scoreToAdd = (int)Math.Floor(totalPrice * 0.01m);
            if (scoreToAdd <= 0) return;
            var score = await _unitOfWork.scoreRepository.GetByUserIdAsync(userId);
            if (score == null)
            {
                score = new Score
                {
                    Userid = userId,
                    TotalScore = scoreToAdd,
                    CreatedAt = DateTime.Now,
                    LastUpdatedAt = DateTime.Now
                };
                await _unitOfWork.scoreRepository.AddAsync(score);
            }
            else
            {
                score.TotalScore += scoreToAdd;
                score.LastUpdatedAt = DateTime.Now;
                await _unitOfWork.scoreRepository.UpdateAsync(score);
            }
            var history = new ScoreHistory
            {
                ScoreId = score.ScoreId,
                ScoreIn = scoreToAdd,
                ScoreOut = 0,
                Description = $"Cộng điểm từ hóa đơn {invoiceId}",
                ChangeDate = DateTime.Now,
                InvoiceId = invoiceId
            };
            await _unitOfWork.scoreHistoryRepository.AddAsync(history);
        }
        /// <summary>
        /// Tính toán discount amount từ điểm (chỉ tính toán, không trừ điểm thực tế)
        /// </summary>
        public async Task<(int scoresUsed, decimal discountAmount)> UseScoreAsync(string userId, int? scoresToUse, decimal totalPrice, int? invoiceId = null)
        {
            var score = await _unitOfWork.scoreRepository.GetByUserIdAsync(userId);
            if (score == null || !scoresToUse.HasValue || scoresToUse.Value <= 0)
                return (0, 0);
            
            int usableScore = Math.Min(score.TotalScore, scoresToUse.Value);
            decimal discount = usableScore; // 1 điểm = 1đ
            if (discount > totalPrice) discount = totalPrice;
            
            // Không trừ điểm thực tế, chỉ tính toán
            // score.TotalScore -= usableScore;
            // score.LastUpdatedAt = DateTime.Now;
            // await _unitOfWork.scoreRepository.UpdateAsync(score);
            
            return (usableScore, discount);
        }
    }

} 