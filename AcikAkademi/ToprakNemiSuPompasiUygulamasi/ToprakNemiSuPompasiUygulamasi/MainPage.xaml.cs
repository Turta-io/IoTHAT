using System.Diagnostics;
using System.Threading;
using TurtaIoTHAT;
using Windows.UI.Xaml.Controls;

namespace ToprakNemiSuPompasiUygulamasi
{
    public sealed partial class MainPage : Page
    {
        // Giriş / Çıkış Portu
        static IOPort gc;

        // Röle Denetleyicisi
        static RelayController role;

        // Ölçüm Timer'ı
        Timer sensorTimer;

        public MainPage()
        {
            this.InitializeComponent();

            // Giriş / Çıkış Portunu başlat
            gc = new IOPort(false, false, false, false);

            // Röle Denetleyicisini başlat
            role = new RelayController();

            // Ölçüm timer'ını yapılandır
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 10000);
        }

        static void SensorTimerTick(object state)
        {
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
        }
    }
}