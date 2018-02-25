# IoT HAT Firmware
IoT HAT'in analog okuma ve kızılötesi kumanda fonksiyonları kart üzerindeki mikrodenetleyici ile sağlanır. Kartınıza bu alanlarda ek özellik gelmişse firmware güncellemesi yaparak yenilikleri kullanabilirsiniz.

## Uyarı
Firmware güncellemesi elektronik ve mikrodenetleyici programlama alanında deneyimli kişiler tarafından yapılmalıdır. Bu süreçte yapacağınız bir hata tüm sisteminizin arızalanmasına neden olabilir. Yükleme işleminiz yarım kalırsa kartınızı onarım için bize gönderebilirsiniz.

## Gereksinimler
Güncelleme yapabilmek için ICSP pinlerine ulaşabildiğiniz bir PIC mikrodenetleyici programlayıcıya ve 5 adet M-M jumper kablosuna ihtiyacınız olacak.

## Bağlantı Şeması
IoT HAT üzerinde bulunan PIC16F182X modeli mikrodenetleyicinin programlama için kullanılan ICSP bağlantıları, kartınızın Raspberry Pi'a bağlanan konnektörüne de bağlıdır. Programlayıcınız ile kartınızın Raspberry Pi konnektörü arasında aşağıda gösterilen bağlantıyı sağladığınızda donanımsal olarak firmware güncellemeye hazır olacaksınız.

// Bağlantı

## Programlayıcı Ayarları
Programlayıcınız üzerinde yapmanız gereken iki önemli ayar bulunur.
- Karta gönderilen elektriği (VDD) 3.3V olarak ayarlayın. (Power sekmesinden voltage settings bölümü, VDD ayarı.)
- Programlayıcının kartı beslemesini sağlayın. (Power sekmesinden "Power target circuit from tool" seçeneğini işaretleyin.)

IoT HAT Serisinde iki farklı model mikrodenetleyici kullanılmıştır. Mikrodeneyleyicinizin model numarasına uyan HEX dosyasını kullanabilirsiniz. (Mikrodenetleyicilerin kullanılan donanım özellikleri aynıdır.)

## Güncellemeler
FW 1.05 (25.02.2018):
- Kızılötesi kumanda vericisinin bazı cihazlarda çalışmadığı farkedildi. NEC Protokolü'ne göre sistem zamanlaması iyileştirildi. Tüm 4-bit NEC Protokolü'nü destekleyen cihazlarla uyumluluk sağlandı.
FW 1.04 (05.04.2017):
- Bazı Raspberry Pi kartlarında I2C iletişimi sırasında yanlış veri aktarılabildiği farkedildi. Bu hata, ADC ve kızılötesi kumanda fonksiyonlarının 64.000 ve 255 değer döndürmesiyle sonuçlanıyordu. I2C İletişiminde yapılan iyileştirmeyle tüm Raspberry Pi 2 / 3 kartlarında düzgün iletişim sağlanıldı.

## Power User'lar için ipuçları
- IoT HAT üzerindeki sensörler ve mikrodenetleyici aynı I2C hattı üzerine bağlıdır. Geliştireceğiniz PIC programı ile Raspberry Pi'a ihtiyaç duymadan kartınızı başka bir sistemle kullanabilirsiniz. Pinlerin 3.3V toleranslı olduğunu unutmayın.

- Mikrodenetleyininin UART pinleri, Raspberry Pi üzerindeki RXD0 / TXD0 pinlerine bağlıdır, ancak firmware üzerinde bu iletişim kullanılmamaktadır. Geliştireceğiniz PIC programı ile UART üzerinden de iletişim sağlayabilirsiniz. Örneğin, tüm sensör hesaplamalarını PIC'e yaptırıp UART üzerinden direkt sonuçları okuyabilirsiniz.

- Kullanılan PIC serilerinde analog girişler üzerinde capacitive sense özelliği bulunur. Geliştireceğiniz PIC programıyla analog girişlerini kapasitif algılama girişleri olarak da kullanabilirsiniz. Bu özelliğin kabloya göre ince ayarının yapılması gerektiğinden, ürün çalışma kalitesinin standardizasyonu nedeniyle capacitive sense özelliği cihazlarda kullanılmamıştır.

- PIC ile kontrol edilen kızılötesi kumanda vericisinin programını kendiniz yazmak isterseniz, kızılötesi verici kontrol bağlantısının PWM çıkışında olduğunu unutmayın. Vericiyi dilerseniz GPIO modunda, dilerseniz PWM modunda kontrol edebiliriniz.
