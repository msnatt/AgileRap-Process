using AgileRap_Process_Software_ModelV2.Models;

public interface IEmailSender
{
    void SendEmail(Message message);
}