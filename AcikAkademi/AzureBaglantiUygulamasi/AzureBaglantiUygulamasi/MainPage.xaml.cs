using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using TurtaIoTHAT;
using Windows.UI.Xaml.Controls;

namespace AzureBaglantiUygulamasi
{
    public sealed partial class MainPage : Page
    {
        // BME280 Hava Durumu Sensörü
        static BME280Sensor bme;

        // VEML6075 Ultraviyole Sensörü
        static VEML6075Sensor veml;

        // Giriş / Çıkış Portu
        static IOPort gc;

        // Röle Denetleyicisi
        static RelayController role;

        // Ölçüm Timer'ı
        Timer sensorTimer;

        public MainPage()
        {
            this.InitializeComponent();

            // BME280 Sensörünü başlat
            bme = new BME280Sensor();

            // VEML6075 Sensörünü başlat
            veml = new VEML6075Sensor();

            // VEML6075 Sensörünü yapılandır
            veml.Config(VEML6075Sensor.IntegrationTime.IT_800ms, VEML6075Sensor.DynamicSetting.Normal, VEML6075Sensor.Trigger.NoActiveForceTrigger, VEML6075Sensor.ActiveForceMode.NormalMode, VEML6075Sensor.PowerMode.PowerOn);

            // Giriş / Çıkış Portunu başlat
            gc = new IOPort(false, false, false, false);

            // Röle Denetleyicisini başlat
            role = new RelayController();

            // Ölçüm timer'ını yapılandır
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 10000);
        }

        static  async void SensorTimerTick(object state)
        {
            // BME280 Sensöründen sıcaklığı oku
            double sicaklik = bme.ReadTemperature();

            // Sıcaklığı yaz
            Debug.WriteLine(sicaklik.ToString("0.000"));

            // VEML6075 Sensöründen UV Index'ini oku
            double uvindex = veml.Calculate_Average_UV_Index();

            // UV Index'i yaz
            Debug.WriteLine(uvindex.ToString("0.000"));

            // 4. Giriş / Çıkış Portundan analog girişi oku
            double toprakNemi = gc.ReadAnalogInput(4, false);

            // Analog değeri yaz
            Debug.WriteLine(toprakNemi.ToString("0.000"));

            // Toprak nemine göre su pompasını aç ya da kapat
            if (toprakNemi > 0.5) // Toprak kuruysa
            {
                // Röleyi aktifleştir: Pompa çalışır.
                role.SetRelay(2, true);
            }
            else // Toprak nemliyse
            {
                // Röleyi pasifleştir: Pompa çalışmaz.
                role.SetRelay(2, false);
            }

            // Azure IoT Hub'a gönderilecek verileri saklayan Telemetri sınıfını oluştur 
            Telemetri telemetri = new Telemetri();
            telemetri.toprakNemi = toprakNemi;
            telemetri.sicaklik = sicaklik;
            telemetri.uvindex = uvindex;

            // Telemetri sınıfını JSON cümlesine çevir
            string telemetriJSON = JsonConvert.SerializeObject(telemetri);

            // Verileri Azure IoT Hub'a gönder
            await AzureIoTHub.SendDeviceToCloudMessageAsync(telemetriJSON);
        }
    }
}