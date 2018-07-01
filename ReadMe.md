# IoT HAT
Turta IoT HAT, Raspberry Pi ve pin uyumlu bilgisayarlara gesture tanımadan dokunma algılamaya birçok özellikte fonksiyon katar. Alanlarında en başarılı bileşenleri tek kartta birleştirerek karmaşık senaryoları kablo karmaşasıyla uğraşmadan kolaylıkla oluşturabilmenizi sağlar.

IoT HAT üzerindeki tüm özelliklere I2C arabirimi ve GPIO denetleyicisi üzerinden erişebilirsiniz. Sensörler I2C1 hattı üzerinden bağlıdır. Sensörlere I2C1 üzerinde 0x76 gibi donanım adreslerinden erişilebilir. Rölelere, izole girişlere ve PIR hareket sensörüne GPIO denetleyicisi üzerinden erişilir. Kızılötesi iletişim ve analog girişler yardımcı mikrodenetleyici ile çalışır, I2C üzerinden haberleşme sağlanır.

## Kullanım Kılavuzu
Bileşenleri nasıl kullanacağınızı anlatan kılavuza Wiki bölümünden erişebilirsiniz. [github.com/Turta-io/IoTHAT/wiki](https://github.com/Turta-io/IoTHAT/wiki "IoT HAT Wiki")

## Donanım Özellikleri
IoT HAT Üzerinde aşağıda belirtilen donanımlar yer alır:

### Bosch Sensortec BME680 Hava Durumu Sensörü
İç alan hava kalitesi, sıcaklık, nem, basınç ve deniz seviyesinden yükseklik ölçümü yapar.
- Hava Kalitesi: 0 - 500 IAQ (Indoor Air Quality) aralığında 1 IAQ çözünürlüğünde iç alan hava kalitesi ölçümler.
- Sıcaklık: -40C - 85C arasında 0.01C çözünürlüğünde sıcaklık ölçümler.
- Nem: %0RH - %100RH arasında, %3 hassasiyetinde, %0.008RH çözünürlüğünde bağıl nem (RH) ölçümler.
- Basınç: 300 - 1100hPa arasında 0.18Pa çözünürlüğünde basınç ölçümler.
- Yükseklik: Anlık havadurumuna göre deniz seviyesi basıncı belirtildiğinde, deniz seviyesine göre yüksekliği hesaplar.

Sensörle I2C 0x76 adresi üzerinden haberleşilir.

*Raspberry Pi'ın ısındığı durumlarda IoT HAT de ısınacağından sıcaklık ölçümü ortamın bir miktar üzerinde algılanır. Bu durumun önüne geçmek için sensör, kartın en az ısınan alanına yerleştirilmiş ve etrafına oluk açılarak karttan izolesi sağlanmıştır. Hassas ölçüm gerektiği durumlarda Raspberry Pi'ınızı dik yerleştirerek ısınan havanın daha verimli tahliyesini sağlayabilirsiniz. Raspbian Lite gibi masaüstü kullanmayan minimal işletim sistemleri Raspberry Pi'ınızı daha az ısıtacaktır.*

*Hava kalitesini IAQ sonucuna göre ölçümlemek için Bosch'un sağladığı algoritmayı kullanmanız gerekir. Yayınladığımız sürücüler gas resistance değeri verir. Hava kalitesi sensörlerinin doğru ölçüm yapabilmesi için kullanılacakları ortamda birkaç gün çalıştırılması gerekir.*

### Avago APDS-9960 Işık, RGB, Gesture ve Mesafe Sensörü
Işık miktarı, kırmızı - yeşil - mavi renk tonları, el hareketinin yönü ve mesafe algılaması yapar.
- Işık miktarı: Sensör üzerine düşen ışık miktarı ölçümlenir. Hassasiyeti API üzerinden ayarlanabilir.
- RGB Renk miktarı: Sensör üzerine düşen kırmızı, yeşil ve mavi renk tonları ölçümlenir. Hassasiyeti API üzerinden ayarlanabilir.
- El hareketi algılama: Sensöre 30cm mesafe dahilinde dört yöne el / obje hareket yönü algılanır.
- Mesafe algılama: 0cm - 30cm aralığında optik olarak dikey mesafe ölçümlenir.

Sensörle I2C 0x39 adresi üzerinden haberleşilir.

### Vishay VEML6075 UV Sensörü
UVA ve UVB değerlerini ölçümler. Buna göre UV A Index, UVB Index ve ortalama UV Index hesaplar.
- UVA: 315nm - 400nm arası dalga boyunda, ozon tabakası tarafından emilmeyen morötesi ışığı ölçümler.
- UVB: 280nm - 315nm arası dalga boyunda, ozon tabakası tarafından bir kısmı emilen morötesi ışığı ölçümler.
- UV Index: UV Radyasyonunu uluslararası ölçüm standardında hesaplar. Bu değere göre güneşin ne süre sonra cilde zarar vermeye başlayacağı hesaplanır.

Sensörle I2C 0x10 adresi üzerinden haberleşilir.

### NXP MMA8491Q İvme ve Eğim Sensörü
3 Eksende ivme ölçümler, eğim algılanması durumunda interrupt üretir.
- İvme: 14-bit +/- 8g ivme verisi 1 mg hassasiyetle ölçümlenir.
- Eğim: 0.688g / 43.5 derece eğimde interrupt üretir. (IoT HAT üzerinde Z ekseni ivme çıkışı bağlıdır.)

Sensörle I2C 0x55 adresi üzerinden haberleşilir.

### AM312 Pasif Kızılötesi Hareket Sensörü
Ortamdaki insan ve hayvanların hareketliliğini algılar.
- Hareket algılama: Isı yayan canlının hareketinden kaynaklı ısı değişimi algılanır.

Sensör durumu GPIO25 pini üzerinden okunur.

### LCA717 Solid State Röle
2 Adet elektronik cihazı açar ya da kapatır.
- Röleler: 2 Adet DC30V 2A gücünde katıhal röle ile elektronik donanımları açıp kapatabilirsiniz.

Röle 1 ve 2 kontrolü sırasıyla GPIO20 ve 12 pinleri üzerinden gerçekleşir.

*Katıhal röleler, mekanik rölelere göre daha uzun ömürlüdür ve hareketli parçaları olmadığı için sessiz çalışırlar. LCA717 dahili optik izolasyona sahiptir. Kart üzerindeki oyuklarla rölelerin elektrik girişleriyle kartın geri kalan kısmı izole edilmiştir.*

*Bağlayacağınız cihazın en yüksek akım değerinin 2 Amper'i geçmemesi gerekir. Örneğin 12V 500mA güç tüketimindeki bir motor çalışmaya başlarken 2 Amper'in üzerinde akım çekebilir. Hızlı tekrar eden aç - kapa işlemi rölenizin ısınmasına ve saniyeler içerisinde arızalanmasına sebep olabilir. Kullanacağınız bileşenin elektrik kullanımını bağlamadan önce kontrol edin, gerekli durumlarda sigorta kullanın.*

### LTV-827S Photocoupler Girişi
4 Adet 5V girişini optik yalıtımla algılar.
- Photocoupler: Dışarıdan verilen 5V girişini izole olarak algılar. Kart üzerindeki olukla girişer kartın geri kalan kısmından izole edilmiştir.

Optokuplör 1, 2, 3 ve 4 girişleri sırasıyla GPIO 13, 19, 16 ve 26 pinleri üzerinden okunur.

### Vishay TSOP75338W Kızılötesi Alıcı ve VSMB10940X01 Kızılötesi Verici
Kızılötesi kumanda verisini okur ve kızılötesi veri gönderir.
- Kızılötesi alıcı: Alıcı modülü 38KHz'de NEC protokolünde 4 Byte'lık veri okur. Veri okunması tamamlandığında GPIO pininde interrupt sinyali oluşur ve I2C üzerinden gelen Byte dizisi okunur.
- Kızılötesi verici: 940nm Dalga boyunda, 104mW gücünde 38KHz NEC protokolünde 4 Byte'lık veri gönderir.

Kızılötesi iletişim I2C 0x28 adresinden sağlanır. Interrupt pini GPIO18'dir.

*Kumanda sistemleri farklı protokollerde çalışır. NEC protokolü kumandalar arasında en yaygın protokoldür. Raspberry Pi kızılötesi iletişim için gerekli tutarlılıkta sinyal üretemediğinden veri alışverişi ve işlemesi kart üzerindeki mikrodenetleyici üzerinde gerçekleşir.*

### ADC
4 Adet analog girişe verilen elektriği ölçerek farklı sensörlerin kullanımını sağlar.
- Analog giriş: 0V ile 3.3V arasında 1/1024 hassasiyetinde ölçüm yapar.

Analog ölçüme I2C 0x28 adresinden erişilir.

*Analog girişlere 3.3V üzerinde elektrik bağlamayın. 3.3V Ölçüm referans değeri üzerindeki elektrik donanımınıza zarar verebilir.*

### I2C ve I/O Soketleri
1 Adet I2C ve 4 adet çok fonksiyonlu I/O bağlantısı sağlar.
- I2C Soketi: I2C bağlantısını dışarıya aktarır. Bu soketle sisteminize farklı sensörler ekleyebilirsiniz. (Kart üzerindeki sensörler adres çakışması olacağından bu hat üzerine tekrardan bağlanamaz. Ancak BME680 sensörünün adresi değiştirilebildiği için ikinci BME680'i bağlayabilirsiniz.)
- I/O Soketleri: Her soket analog giriş ve Raspberry Pi GPIO'larına bağlı birer dijital pin içerir. GPIO pinlerini kullanarak buton, röle gibi bileşenleri sisteminize ekleyebilirsiniz.

I/O Soketlerindeki GPIO pinlerinin numaraları sırasıyla GPIO 21, 22, 23 ve 24'dür.

## Yazılım Desteği
IoT HAT, GPIO ve I2C erişimi sağlayan tüm işletim sistemlerinde kullanılabilir. Yayınladığımız sürücüler dışındaki 3. parti sürücüleri de kullanabilirsiniz.

## Sanayi 4.0 Eğitimi
Nesnelerin İnterneti kavramına hızlı giriş yapabilmeniz ve modern geliştirme teknolojilerini yakalamanız için Microsoft işbirliğiyle 16 saatlik video eğitimi hazırladık. Bu seride Raspberry Pi için Windows 10 IoT Core yüklenmesini, temel bir UWP uygulaması geliştirmeyi, Raspberry Pi'da uygulama çalıştırmayı, Azure IoT Hub'a veri göndermeyi ve Power BI'da verileri göreceksiniz. Eğitime https://www.acikakademi.com/portal/egitimler/sanayi-4-0.aspx adresinden erişebilirsiniz.

## Etkinliklerimiz
Çoğunlukla İstanbul'da olmak üzere bir çok etkinlik düzenliyoruz. Etkinliklerimizde tanışmak için Meet-up üzerinden "İstanbul IoT & Wearables" grubuna üye olabilirsiniz. https://meetup.com/istiot

## Açıklamalar
Belirtilen GPIO pin numaraları, GPIO Denetleyicisinde yazılımsal olarak belirlediğiniz GPIO bağlantı numaralarıdır. Raspberry Pi header'ı üzerindeki pin numarası değildir.

IoT HAT üzerindeki sensörler endüstriyel standartlarda, alanlarının en başarılı sensörleridir. Ancak bu sistem eğitim amaçlıdır. Güvenlik ve medikal alanda kullanılmamalıdır. Herhangi bir arıza durumunda başka bir donanımı veya hayatı tehlikeye atacak görevlerde kullanılmamalıdır. Kartın kullanımındaki tüm sorumluluk kullanıcıya aittir.
