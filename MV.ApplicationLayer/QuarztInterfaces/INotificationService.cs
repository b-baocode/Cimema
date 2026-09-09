namespace MV.ApplicationLayer.QuarztInterfaces
{
    public interface INotificationService
    {
        Task SendMessageToGroupAsync(string groupName, string message);
    }
}
