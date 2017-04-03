using System.Diagnostics;
using System.Threading;
using TurtaIoTHAT;
using Windows.UI.Xaml.Controls;

namespace ToprakNemiUygulamasi
{
    public sealed partial class MainPage : Page
    {
        // Giriş / Çıkış Portu
        static IOPort gc;

        // Ölçüm Timer'ı
        Timer sensorTimer;

        public MainPage()
        {
            this.InitializeComponent();

            // Giriş / Çıkış Portunu başlat
            gc = new IOPort(false, false, false, false);

            // Ölçüm timer'ını yapılandır
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 2000);
        }

        static void SensorTimerTick(object state)
        {
            // 4. Giriş / Çıkış Portundan analog girişi oku
            double toprakNemi = gc.ReadAnalogInput(4, false);

            // Analog değeri yaz
            Debug.WriteLine(toprakNemi.ToString("0.000"));
        }
    }
}