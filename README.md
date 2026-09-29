## 🏗️ Mimari ve Tasarım Desenleri (Architecture & Patterns)

* **Clean Architecture & Dependency Injection (DI):** 
  İş mantığı (Business Logic), dış dünyadan ve altyapı bağımlılıklarından tamamen soyutlanmıştır. Dependency Injection sayesinde katmanlar arası bağımlılıklar arayüzler (interface) üzerinden yönetilerek gevşek bağlı (loosely coupled) bir yapı elde edilmiştir. Bu sayede ileride veritabanı veya harici servis değişiklikleri ana iş kodlarına dokunulmadan kolayca gerçekleştirilebilir.

* **Mediator Design Pattern:** 
  Sunum katmanı (Presentation/Controller) ile iş mantığı katmanı arasındaki doğrudan bağımlılığı ortadan kaldırmak için **Mediator** tasarım kalıbı kullanılmıştır. Gelen tüm istekler bir komut (Command) veya sorgu (Query) olarak merkezi işleyicilere (Handler) yönlendirilerek kodun okunabilirliği, test edilebilirliği ve bakımı kolaylaştırılmıştır.

---

## 🗄️ Veritabanı ve Depolama Stratejisi

* **İlişkisel Veritabanı (MSSQL):** 
  Veritabanı tabloları arasında bire-çok (1-N) ilişkiler kurgulanmıştır. Geliştirme ortamında tutarlılık ve hızlı kurulum sağlamak amacıyla **MSSQL**, **Docker Container** üzerinde yapılandırılarak çalıştırılmıştır.

* **Dosya Saklama (File Storage):** 
  Projede ilk aşamada nesne depolama çözümü olarak **MinIO Object Storage** kullanılması planlanmıştır. Ancak MinIO'nun son sürümlerindeki lisanslama ve ücret politikası değişiklikleri göz önünde bulundurularak, dosya saklama yaklaşımı olarak sunucu tarafında yerel **`wwwroot`** dizini tercih edilmiştir.

---

## 📄 PDF Oluşturma (Document Generation)

* **QuestPDF:** 
  Yüklenen Word/belge içeriklerinin standart ve yüksek performanslı e-kitap (PDF) çıktılarına dönüştürülmesi sürecinde **QuestPDF** kütüphanesinden yararlanılmıştır.

---

## 🛡️ Güvenlik ve Performans (Security & Optimization)

* **API Rate Limiting:** 
  API kaynaklarını aşırı yüklenmeye ve kötüye kullanıma (abuse) karşı korumak amacıyla .NET yerleşik **`UseRateLimiter`** middleware'i entegre edilmiştir. Yapılandırılan *Fixed Window* politikası doğrultusunda, aynı IP adresine sahip kullanıcılara **dakikada maksimum 5 istek (5 requests/minute)** limiti uygulanmıştır.

---

## ⚙️ Proje Standartları ve Konfigürasyon

* **Güvenlik ve Hassas Veri Yönetimi (`.gitignore`):** 
  Hassas verilerin ve veritabanı bağlantı cümlelerinin (connection string) versiyon kontrol sistemine sızmasını önlemek amacıyla `appsettings.json` ve `appsettings.Development.json` gibi yapılandırma dosyaları `.gitignore` kapsamına alınmıştır.

* **Repository Temizliği:** 
  Depo boyutunu optimize etmek, çakışmaları önlemek ve derleme çıktılarını versiyon kontrolü dışında tutmak için `bin/`, `obj/`  gibi derleme/bağımlılık klasörleri `.gitignore` dosyasına eklenmiştir.
