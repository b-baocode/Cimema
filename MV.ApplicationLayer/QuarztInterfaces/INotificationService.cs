using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.QuarztInterfaces
{
    public interface INotificationService
    {
        Task SendMessageToGroupAsync(string groupName, string message);
    }
}
