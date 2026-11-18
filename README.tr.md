[English](README.md) · **Türkçe**

# Sinek Yakalama

Görsel Programlama dersi ödevi olarak C# Windows Forms ile yapılmış sinek avlama oyunu. Bütün çizimler GDI+ ile kodla yapılır.

## Nasıl oynanır
- Düğmeye basınca 30 saniyelik tur başlar.
- Sineklere tıklayıp raketle vur: her sinek **+1** puan.
- Ara sıra kısa süreliğine **altın sinek** gelir: **+3** puan.
- Puanın arttıkça sinekler hızlanır. Sonunda puan, isabet, ıska ve rekor gösterilir.

## Çalıştırma
- **Windows:** `gorsel programlama 2 - ders1 - sinek yakalama.slnx` dosyasını Visual Studio'da aç, F5'e bas (.NET Framework 4.7.2).
- **Windows olmadan hızlıca bakmak için:** `tarayici-onizleme.html` dosyasını tarayıcıda aç.

## Dosyalar
- `Form1.cs` — oyun mantığı, sineklerin uçuşu, fare ve bütün çizimler (gökyüzü, tepeler, çiçekler, sinekler)
- `Form1.Designer.cs` — form ve düğme yerleşimi
- `Program.cs` — giriş noktası
