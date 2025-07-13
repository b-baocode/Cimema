# Chức năng Comment Rating với điều kiện xem phim

## Mô tả
Chức năng Comment Rating đã được cập nhật để yêu cầu người dùng phải thanh toán và xem phim trước khi có thể đánh giá. Điều này đảm bảo tính chính xác và độ tin cậy của các đánh giá.

## Thay đổi chính

### 1. Thêm method kiểm tra trong Repository
**File**: `MV.InfrastructureLayer/Repositories/CommentRatingRepository.cs`

```csharp
public async Task<bool> HasUserWatchedMovieAsync(string userId, int movieId)
{
    // Kiểm tra xem người dùng có vé đã thanh toán thành công cho phim này không
    var hasWatchedMovie = await _context.TicketInvoices
        .Include(ti => ti.TicketDetails)
        .ThenInclude(td => td.ShowtimeInstance)
        .ThenInclude(sri => sri.Showtime)
        .Where(ti => ti.Userid == userId && 
                   ti.Status == "Success" && // Chỉ tính những vé đã thanh toán thành công
                   ti.TicketDetails.Any(td => 
                       td.ShowtimeInstance.Showtime.MovieId == movieId))
        .AnyAsync();

    return hasWatchedMovie;
}
```

### 2. Thêm Exception mới
**File**: `MV.ApplicationLayer/SpecificExceptionReport/UserHasNotWatchedMovieException.cs`

```csharp
public class UserHasNotWatchedMovieException : Exception
{
    public UserHasNotWatchedMovieException(string message) : base(message) { }
    public UserHasNotWatchedMovieException(string message, Exception innerException) : base(message, innerException) { }
}
```

### 3. Cập nhật Service logic
**File**: `MV.ApplicationLayer/Services/CommentRatingService.cs`

```csharp
public async Task<CommentRatingResponse> CreateAsync(CommentRatingRequest request)
{
    // Kiểm tra xem user đã đánh giá phim này chưa
    var existingComment = await _unitOfWork.commentRatingRepository.GetByUserIdAndMovieIdAsync(request.UserId, request.MovieId);
    if (existingComment != null)
    {
        throw new CommentAlreadyExistsException("You have already commented on this movie.");
    }

    // Kiểm tra xem user đã thanh toán và xem phim này chưa
    var hasWatchedMovie = await _unitOfWork.commentRatingRepository.HasUserWatchedMovieAsync(request.UserId, request.MovieId);
    if (!hasWatchedMovie)
    {
        throw new UserHasNotWatchedMovieException("You must purchase and watch this movie before you can rate it.");
    }

    // Tiếp tục tạo đánh giá nếu đã xem phim
    // ...
}
```

### 4. Cập nhật Controller để xử lý Exception
**File**: `MV.PresnetationLayer/Controllers/CommentRatingController.cs`

```csharp
[HttpPost]
[Authorize]
public async Task<ActionResult<CommentRatingResponse>> Create(CommentRatingRequest request)
{
    try
    {
        var commentRating = await _commentRatingService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByMovieId), new { movieId = commentRating.MovieId }, commentRating);
    }
    catch (CommentAlreadyExistsException ex)
    {
        return Conflict(new { message = ex.Message }); // 409 Conflict
    }
    catch (UserHasNotWatchedMovieException ex)
    {
        return BadRequest(new { message = ex.Message }); // 400 Bad Request
    }
    catch (UniqueConstraintViolationException ex)
    {
        return Conflict(new { message = ex.Message }); // 409 Conflict
    }
}
```

## Logic kiểm tra

### Điều kiện để được đánh giá:
1. **Người dùng phải đăng nhập** (có `[Authorize]`)
2. **Chưa đánh giá phim này trước đó** (kiểm tra `CommentRating` table)
3. **Đã thanh toán vé xem phim thành công** (kiểm tra `TicketInvoice` với status = "Success")
4. **Vé phải thuộc về phim cần đánh giá** (kiểm tra qua relationship: `TicketInvoice` → `TicketDetail` → `ShowtimeRoomInstance` → `Showtime` → `Movie`)

### Luồng kiểm tra:
```
User Request → Check if already rated → Check if watched movie → Create rating
     ↓                    ↓                        ↓
[Authorize]    [CommentAlreadyExists]    [UserHasNotWatchedMovie]
     ↓                    ↓                        ↓
   OK             409 Conflict              400 Bad Request
```

## API Response

### Thành công (201 Created)
```json
{
    "commentRatingId": 1,
    "userId": "user123",
    "userName": "John Doe",
    "movieId": 5,
    "movieName": "Avengers: Endgame",
    "rating": 5,
    "comment": "Great movie!",
    "createdAt": "2024-01-15T10:30:00"
}
```

### Lỗi - Chưa xem phim (400 Bad Request)
```json
{
    "message": "You must purchase and watch this movie before you can rate it."
}
```

### Lỗi - Đã đánh giá trước đó (409 Conflict)
```json
{
    "message": "You have already commented on this movie."
}
```

## Database Relationships

Để kiểm tra người dùng đã xem phim, hệ thống sử dụng các relationship sau:

```
TicketInvoice (UserId, Status = "Success")
    ↓
TicketDetail (InvoiceId)
    ↓
ShowtimeRoomInstance (ShowtimeInstanceId)
    ↓
Showtime (ShowtimeId)
    ↓
Movie (MovieId)
```

## Lợi ích

1. **Tính chính xác**: Chỉ những người thực sự xem phim mới được đánh giá
2. **Chống spam**: Ngăn chặn đánh giá giả từ người chưa xem phim
3. **Độ tin cậy**: Tăng độ tin cậy của hệ thống đánh giá
4. **Trải nghiệm người dùng**: Đảm bảo đánh giá phản ánh đúng trải nghiệm thực tế

## Testing

Để test chức năng này:

1. **Test case 1**: User chưa mua vé → Gọi API → Nhận 400 Bad Request
2. **Test case 2**: User đã mua vé nhưng chưa thanh toán → Gọi API → Nhận 400 Bad Request  
3. **Test case 3**: User đã thanh toán thành công → Gọi API → Nhận 201 Created
4. **Test case 4**: User đã đánh giá trước đó → Gọi API → Nhận 409 Conflict 