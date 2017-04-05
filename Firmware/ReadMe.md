# IoT HAT Firmware
IoT HAT'in analog okuma ve kızılötesi kumanda fonksiyonları kart üzerindeki mikrodenetleyici ile sağlanır. Kartınıza bu alanlarda ek özellik gelmişse firmware güncellemesi yaparak yenilikleri kullanabilirsiniz.

## Uyarı
_Firmware güncellemesi mikrodenetleyici programlama alanında deneyimli kişiler tarafından yapılmalıdır. Bu süreçte yapacağınız bir hata tüm sisteminizin arızalanmasına neden olabilir._

## Gereksinimler
Güncelleme yapabilmek için ICSP pinlerine ulaşabildiğiniz bir PIC programlayıcıya (örneğin PICKit 3) ve 5 adet M-M jumper kablosuna ihtiyacınız olacak.

## Bağlantı Şeması
IoT HAT üzerinde bulunan PIC16F182X modeli mikrodenetleyicinin ICSP bağlantısı, kartın arkasındaki 40 pinli konnektöre bağlıdır. PIC Programlayıcınız ile kartınızın 40 pin konnektörü arasında bağlantı sağladığınızda donanımsal olarak firmware güncellemesine hazır olacaksınız.

![IoTHAT ICSP Bağlantısı](http://turta.io/githubimg/IoTHAT_ICSP.png)

IoT HAT Header Pin 12 -> ICSP CLK  
IoT HAT Header Pin 13 -> ICSP DAT  
IoT HAT Header Pin 17 -> ICSP 3.3V  
IoT HAT Header Pin 20 -> ICSP GND  
IoT HAT Header Pin 26 -> ICSP MCLR  

## Programlayıcı Ayarları
Programlayıcınız üzerinde yapmanız gereken iki önemli ayar bulunur.
- Karta gönderilen elektriği (VDD) 3.3V olarak ayarlayın. (PICKit 3 için power sekmesinden voltage settings bölümü, VDD ayarı.)
- Programlayıcının kartı beslemesini sağlayın. (PICKit 3 için power sekmesinden "Power target circuit from tool" seçeneğini işaretleyin.)

IoT HAT Serisinde iki farklı model mikrodenetleyici kullanılmıştır. Mikrodeneyleyicinizin model numarasına uyan HEX dosyasını kullanabilirsiniz. (Mikrodenetleyicilerin kullanılan donanım özellikleri aynıdır.)

## Güncellemeler
FW 1.04:
- Analog okuma ve kızılötesi iletişim değerlerinin I2C iletişiminde bazı Raspberry Pi kartlarına hatalı aktarımı düzeltildi.

## Power User ve Hacker'lar için İpuçları
- IoT HAT üzerindeki sensörler ve mikrodenetleyici aynı I2C hattı üzerine bağlıdır. Geliştireceğiniz PIC programı ile Raspberry Pi'a ihtiyaç duymadan kartınızı Arduino gibi başka bir sistemle kullanabilirsiniz. Pinlerin 3.3V toleranslı olduğunu unutmayın.

- Mikrodenetleyicinizin UART pinleri Raspberry Pi üzerindeki RXD0 / TXD0 pinlerine bağlıdır, ancak firmware üzerinde bu iletişim kullanılmamaktadır. Geliştireceğiniz PIC programı ile UART üzerinden de iletişim sağlayabilirsiniz. Örneğin, tüm sensör hesaplamalarını PIC'e yaptırıp UART üzerinden direkt sonuçları okuyabilirsiniz.

- Kullanılan PIC serilerinde analog girişler üzerinde capacitive sense özelliği de bulunur. Geliştireceğiniz PIC programıyla analog girişleri kapasitif algılama girişleri olarak da kullanabilirsiniz. Bu özelliğin kullanılacak kabloya göre ince ayarının yapılması gerektiğinden ürün çalışma kalitesinin standardizasyonu nedeniyle capacitive sense özelliği cihazlarda kullanılmamıştır.

- PIC ile kontrol edilen kızılötesi kumanda vericisinin programını kendiniz yazmak isterseniz IR LED kontrol bağlantısının PWM çıkışında olduğunu unutmayın. Vericiyi dilerseniz GPIO modunda, dilerseniz PWM modunda kontrol edebiliriniz.

- PIC'i besleyen 3.3V girişiyle IoT HAT'in diğer tüm bileşenlerini besleyen 3.3V girişi farklı pinlere bağlıdır. Bu ayrım PIC programlanırken sensörlerin araya girerek iletişimi bozmalarını engellemek için yapılmıştır. Bazı sensörlerin datasheetlerinde elektrik bağlantıları yapılmadan iletişim pinlerine elektrik verildiğinde arızalandıkları belirtilmiştir. Bu nedenle programlama dışındaki durumlarda IoT HAT'inizin tüm 3.3V, 5V ve GND pinlerinin bağlı olduğundan emin olun.
