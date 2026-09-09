namespace MV.ApplicationLayer.HelperMethodsForThirdParty
{
    public interface ISeatNotificationService
    {
        Task SendMessageToGroupAsync(string groupName, string message);
    }
}
