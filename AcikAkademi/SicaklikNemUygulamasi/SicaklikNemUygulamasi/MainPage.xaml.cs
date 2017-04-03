using System.Diagnostics;
using System.Threading;
using TurtaIoTHAT;
using Windows.UI.Xaml.Controls;

namespace SicaklikNemUygulamasi
{
    public sealed partial class MainPage : Page
    {
        // BME280 Hava Durumu Sensörü
        static BME280Sensor bme;

        // Ölçüm Timer'ı
        Timer sensorTimer;
        
        public MainPage()
        {
            this.InitializeComponent();

            // BME280 Sensörünü başlat
            bme = new BME280Sensor();

            // Ölçüm timer'ını yapılandır
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 2000);
        }

        static void SensorTimerTick(object state)
        {
            // BME280 Sensöründen sıcaklığı oku
            double sicaklik = bme.ReadTemperature();

            // Sıcaklığı yaz
            Debug.WriteLine(sicaklik.ToString("0.000"));
        }
    }
}