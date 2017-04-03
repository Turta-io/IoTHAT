using System.Diagnostics;
using System.Threading;
using TurtaIoTHAT;
using Windows.UI.Xaml.Controls;

namespace SicaklikNemUltraviyoleUygulamasi
{
    public sealed partial class MainPage : Page
    {
        // BME280 Hava Durumu Sensörü
        static BME280Sensor bme;

        // VEML6075 Ultraviyole Sensörü
        static VEML6075Sensor veml;

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

            // Ölçüm timer'ını yapılandır
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 2000);
        }

        static void SensorTimerTick(object state)
        {
            // BME280 Sensöründen sıcaklığı oku
            double sicaklik = bme.ReadTemperature();

            // Sıcaklığı yaz
            Debug.WriteLine(sicaklik.ToString("0.000"));

            // VEML6075 Sensöründen UV Index'ini oku
            double uvindex = veml.Calculate_Average_UV_Index();

            // UV Index'i yaz
            Debug.WriteLine(uvindex.ToString("0.000"));
        }
    }
}