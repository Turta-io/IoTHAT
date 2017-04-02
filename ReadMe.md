# IoT HAT
Turta IoT HAT, Raspberry Pi 3 ve pin uyumlu bilgisayarlara gesture tanımadan dokunma algılamaya birçok özellikte fonksiyon katar. Alanlarında en başarılı bileşenleri tek kartta birleştirerek karmaşık senaryoları kablo karmaşasıyla uğraşmadan kolaylıkla oluşturabilmenizi sağlar.

IoT HAT üzerindeki tüm özelliklere I2C arabirimi ve GPIO denetleyicisi üzerinden erişebilirsiniz. Sensörler I2C1 hattı üzerinden bağlıdır. Sensörlere I2C1 üzerinde 0x77 gibi donanım adreslerinden erişilebilir. Rölelere, optokupler girişlerine ve hareket sensörüne GPIO denetleyicisi üzerinden erişilir. Kızılötesi iletişim, analog ve kapasitif girişler yardımcı mikrodenetleyici ile çalışır ve I2C üzerinden haberleşme sağlanır.

## Kullanım Kılavuzu
Bileşenleri nasıl kullanacağınızı anlatan kılavuza Wiki bölümünden erişebilirsiniz. [github.com/Turta-io/IoTHAT/wiki](https://github.com/Turta-io/IoTHAT/wiki "IoT HAT Wiki")

## Donanım Özellikleri
IoT HAT Üzerinde aşağıda belirtilen donanımlar yer alır:

### Bosch Sensortec BME280 Hava Durumu Sensörü
Sıcaklık, nem, basınç ve deniz seviyesinden yükseklik ölçümü yapar.
- Sıcaklık: -40C - 85C arasında 0.01C çözünürlüğünde sıcaklık ölçümler.
- Nem: %0RH - %100RH arasında %0.008RH çözünürlüğünde bağın nem (RH) ölçümler.
- Basınç: 300 - 1100hPa arasında 0.18Pa çözünürlüğünde basınç ölçümler.
- Yükseklik: Anlık havadurumuna göre deniz seviyesi basıncı belirtildiğinde, deniz seviyesine göre yüksekliği hesaplar.

Sensörle I2C 0x77 adresi üzerinden haberleşilir.

*Raspberry Pi'ın ısındığı durumlarda IoT HAT de ısınacağından sıcaklık ölçümü ortamın bir miktar üzerinde algılanır. Bu durumun önüne geçmek için sensör, kartın en az ısınan alanına yerleştirilmiş ve etrafına oyuk açılarak karttan izolesi sağlanmıştır. Hassas ölçüm gerektiği durumlarda Raspberry Pi'ınızı dik yerleştirerek ısınan havanın daha verimli tahliyesini sağlayabilirsiniz.*

### Avago APDS-9960 Işık, RGB, Gesture ve Mesafe Sensörü
Işık miktarı, kırmızı - yeşil - mavi renk tonları, el hareketinin yönü ve mesafe algılaması yapar.
- Işık miktarı: Sensör üzerine düşen ışık miktarı ölçümlenir. Hassasiyeti API üzerinden ayarlanabilir.
- RGB Renk miktarı: Sensör üzerine düşen kırmızı, yeşil ve mavi renk tonları ölçümlenir. Hassasiyeti API üzerinden ayarlanabilir.
- El hareketi algılama: Sensöre 30cm mesafe dahilinde dört yöne el / obje hareket yönü algılanır.
- Mesafe algılama: 0cm - 30cm aralığında optik olarak dikey mesafe ölçümlenir.

Sensörle I2C 0x39 adresi üzerinden haberleşilir.

### Maxim MAX30100 Nabız ve Oksijen Sensörü
Optik olarak parmaktan nabız ve kandaki oksijen miktarını ölçümler.
- Nabız: İşaret parmağından nabız ölçümü yapar. Kızılötesi LED ve alıcı ile ölçüm sağlanır.
- SPO2: Nabız ölçümü ile birlikte kandaki oksijen oranı da ölçümlenir. Kırmızı LED ve alıcı ile ölçüm sağlanır.

Sensörle I2C 0x57 adresi üzerinden haberleşilir.

### Vishay VEML6075 UV Sensörü
UVA ve UVB değerlerini ölçümler. Buna göre UV A Indeks, UVB Indeks ve ortalama UV Indeks hesaplar.
- UVA: 315nm - 400nm arası dalga boyunda, ozon tabakası tarafından emilmeyen morötesi ışığı ölçümler.
- UVB: 280nm - 315nm arası dalga boyunda, ozon tabakası tarafından bir kısmı emilen morötesi ışığı ölçümler.
- UV Indeks: UV Radyasyonunu uluslararası ölçüm standardında hesaplar. Bu değere göre güneşin ne süre sonra cilde zarar vermeye başlayacağı hesaplanır.

Sensörle I2C 0x10 adresi üzerinden haberleşilir.

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

### PC817 Optokuplör Girişi
4 Adet elektrik girişini algılar.
- Optokuplörler: Dışarıdan verilen 5V girişini izole olarak algılar. Kart üzerindeki oyukla optokuplör girişleri kartın geri kalan kısmıyla izole edilmiştir.

Optokuplör 1, 2, 3 ve 4 girişleri sırasıyla GPIO 13, 19, 16 ve 26 pinleri üzerinden okunur.

### Vishay TSOP38338 Kızılötesi Alıcı ve QEE133 Kızılötesi Verici
Kızılötesi kumanda verisini okur ve kızılötesi veri gönderir.
- Kızılötesi alıcı: Alıcı modülü 38KHz'de NEC protokolünde 4 Byte'lık veri okur. Veri okunması tamamlandığında GPIO pininde interrupt sinyali oluşur ve I2C üzerinden gelen Byte dizisi okunur.
- Kızılötesi verici: 100mW gücünde 38KHz NEC protokolünde 4 Byte'lık veri gönderir.

Kızılötesi iletişim I2C 0x28 adresinden sağlanır. Interrupt pini GPIO18'dir.

*Kumanda sistemleri farklı protokollerde çalışır. NEC protokolü kumandalar arasında en yaygın protokoldür. Raspberry Pi kızılötesi iletişim için gerekli tutarlılıkta sinyal üretemediğinden veri alışverişi ve işlemesi kart üzerindeki mikrodenetleyici üzerinde gerçekleşir.*

### ADC
4 Adet analog girişe verilen elektriği ölçerek farklı sensörlerin kullanımını sağlar.
- Analog giriş: 0V ile 3.3V arasında 1/1024 hassasiyetinde ölçüm yapar.

Analog ölçüme I2C 0x28 adresinden erişilir.

### I2C ve G/Ç Soketleri
2 Adet I2C ve 4 adet çok fonksiyonlu G/Ç bağlantısı sağlar.
- I2C Soketi: 2 Adet I2C bağlantısını dışarıya aktarır. Bu soketlerle sisteminize farklı sensörler ekleyebilirsiniz. (Kart üzerindeki sensörler adres çakışması olacağından bu hat üzerine tekrardan bağlanamaz. Ancak BME280 sensörünün adresi değiştirilebildiği için ikinci BME280'i bağlayabilirsiniz.)
- G/Ç Soketi: Her soket analog girişle paylaşımlı kapasitif giriş ve Raspberry Pi GPIO'larına bağlı birer dijital pin içerir. GPIO pinlerini kullanarak buton, röle gibi bileşenleri sisteminize ekleyebilirsiniz.

G/Ç Soketlerindeki GPIO pinlerinin numaraları sırasıyla GPIO 21, 22, 23 ve 24'dür.

## Yazılım Desteği
IoT HAT, GPIO ve I2C erişimi sağlayan tüm işletim sistemlerinde kullanılabilir. Başlangıçta Windows 10 IoT Core için C# / UWP için sürücü ve örnek uygulamalar paylaşılmıştır. İlerleyen zamanda diğer platformlar için de örnekler paylaşılacaktır. Kullandığınız platform için geliştirilmiş 3. parti sürücüleri de kullanabilirsiniz.

## Endüstri 4.0 Eğitimi
Nesnelerin İnterneti kavramına hızlı giriş yapabilmeniz ve modern geliştirme teknolojilerini yakalamanız için Microsoft işbirliğiyle 16 saatlik video eğitimi hazırladık. Bu seride Raspberry Pi için Windows 10 IoT Core yüklenmesini, temel bir UWP uygulaması geliştirmeyi, Raspberry Pi'da uygulama çalıştırmayı, Azure IoT Hub'a veri göndermeyi ve Power BI'da verileri göreceksiniz. Eğitime https://www.acikakademi.com/portal/egitimler/sanayi-4-0.aspx adresinden erişebilirsiniz.

## Etkinliklerimiz
Çoğunlukla İstanbul'da olmak üzere bir çok etkinlik düzenliyoruz. Etkinliklerimizde tanışmak için Meet-up üzerinden "İstanbul IoT & Wearables" grubuna üye olabilirsiniz. https://meetup.com/istiot

## Açıklamalar
Belirtilen GPIO pin numaraları, GPIO Denetleyicisinde yazılımsal olarak belirlediğiniz GPIO bağlantı numaralarıdır. Raspberry Pi header'ı üzerindeki pin numarası değildir.

IoT HAT üzerindeki sensörler endüstriyel standartlarda, alanlarının en başarılı sensörleridir. Ancak bu sistem eğitim amaçlıdır. Güvenlik ve medikal alanda kullanılmamalıdır. Herhangi bir arıza durumunda başka bir donanımı veya hayatı tehlikeye atacak görevlerde kullanılmamalıdır. Kartın kullanımındaki tüm sorumluluk kullanıcıya aittir.
