using Microsoft.Azure.Devices.Client;
using System.Text;
using System.Threading.Tasks;

namespace AzureBaglantiUygulamasi
{
    static class AzureIoTHub
    {
        const string deviceConnectionString = "HostName=xxxxx.azure-devices.net;DeviceId=xxxxx;SharedAccessKey=xxxxx";

        public static async Task SendDeviceToCloudMessageAsync(string mesaj)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);

            var message = new Message(Encoding.ASCII.GetBytes(mesaj));
            await deviceClient.SendEventAsync(message);
        }
    }
}