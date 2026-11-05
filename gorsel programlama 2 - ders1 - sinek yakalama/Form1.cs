using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace gorsel_programlama_2___ders1___sinek_yakalama
{
    public partial class Form1 : Form
    {
        // ---- Bir sinek ----
        class Sinek
        {
            public float X, Y;           // merkezi
            public float HedefX, HedefY; // uçtuğu nokta
            public float Aci;            // baktığı yön (radyan)
            public float Kanat;          // kanat çırpma sayacı
            public bool Altin;           // altın sinek +3 puan
            public int Omur;             // altın sinek ne kadar kalacak (tik)
        }

        // ---- Vurunca çıkan leke ve "+1" yazısı ----
        class Efekt
        {
            public float X, Y;
            public int Yas;      // kaç tiktir ekranda
            public string Yazi;
            public Color Renk;
            public bool Leke;
        }

        Random rastgele = new Random();
        int skor = 0;
        int sure = 30;
        int rekor = 0;
        int vurus = 0, iska = 0;

        enum Durum { Giris, Oynuyor, Bitti }
        Durum durum = Durum.Giris;

        Sinek sinek;
        Sinek altinSinek;
        int altinSayac;
        List<Efekt> efektler = new List<Efekt>();
        List<PointF> bulutlar = new List<PointF>();
        int tik = 0;

        Point fare = new Point(-100, -100);
        int raketVurus = 0;   // raket inme animasyonu

        const int UstBar = 76;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            for (int i = 0; i < 5; i++)
                bulutlar.Add(new PointF(rastgele.Next(0, ClientSize.Width), rastgele.Next(UstBar + 20, 260)));

            sinek = YeniSinek(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ButonuOrtala();
            timer1.Start();   // animasyon her zaman çalışır (girişte de sinek uçuşur)
        }

        void ButonuOrtala()
        {
            button1.Left = (ClientSize.Width - button1.Width) / 2;
            button1.Top = ClientSize.Height / 2 + 70;
        }

        // ---------------- Oyun akışı ----------------
        private void button1_Click(object sender, EventArgs e)
        {
            skor = 0; sure = 30; vurus = 0; iska = 0;
            efektler.Clear();
            sinek = YeniSinek(false);
            altinSinek = null;
            altinSayac = rastgele.Next(150, 300);
            durum = Durum.Oynuyor;
            button1.Visible = false;
            Cursor.Hide();
            timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            sure--;
            if (sure <= 0)
            {
                sure = 0;
                timer2.Stop();
                durum = Durum.Bitti;
                if (skor > rekor) rekor = skor;
                Cursor.Show();
                button1.Text = "TEKRAR OYNA";
                button1.Visible = true;
                altinSinek = null;
            }
        }

        Sinek YeniSinek(bool altin)
        {
            Sinek s = new Sinek();
            s.X = rastgele.Next(60, ClientSize.Width - 60);
            s.Y = rastgele.Next(UstBar + 60, ClientSize.Height - 60);
            s.Altin = altin;
            s.Omur = 240;
            HedefSec(s);
            return s;
        }

        void HedefSec(Sinek s)
        {
            s.HedefX = rastgele.Next(50, ClientSize.Width - 50);
            s.HedefY = rastgele.Next(UstBar + 40, ClientSize.Height - 40);
        }

        // Sineği hedefine doğru uçurur; skor arttıkça hızlanır
        void Ucur(Sinek s, float hiz)
        {
            float dx = s.HedefX - s.X, dy = s.HedefY - s.Y;
            float mesafe = (float)Math.Sqrt(dx * dx + dy * dy);
            if (mesafe < 20 || rastgele.Next(0, 90) == 0) { HedefSec(s); return; }

            // vızıldama: yönüne dik küçük titreşim
            float titre = (float)Math.Sin(tik * 0.5 + s.X * 0.01) * 1.6f;
            float yx = dx / mesafe, yy = dy / mesafe;
            s.X += yx * hiz - yy * titre;
            s.Y += yy * hiz + yx * titre;

            // yönü yumuşakça döndür
            float hedefAci = (float)Math.Atan2(dy, dx);
            float fark = hedefAci - s.Aci;
            while (fark > Math.PI) fark -= (float)(2 * Math.PI);
            while (fark < -Math.PI) fark += (float)(2 * Math.PI);
            s.Aci += fark * 0.2f;
            s.Kanat += 1.4f;
        }

        // Animasyon zamanlayıcısı (yaklaşık 50 kare/saniye)
        private void timer1_Tick(object sender, EventArgs e)
        {
            tik++;

            float hiz = durum == Durum.Oynuyor ? Math.Min(11f, 3.5f + skor * 0.25f) : 2.5f;
            Ucur(sinek, hiz);

            if (durum == Durum.Oynuyor)
            {
                // altın sinek ara sıra gelir, kısa süre kalır
                if (altinSinek == null)
                {
                    altinSayac--;
                    if (altinSayac <= 0) altinSinek = YeniSinek(true);
                }
                else
                {
                    Ucur(altinSinek, hiz + 3);
                    altinSinek.Omur--;
                    if (altinSinek.Omur <= 0)
                    {
                        altinSinek = null;
                        altinSayac = rastgele.Next(200, 350);
                    }
                }
            }

            // bulutlar yavaşça kayar
            for (int i = 0; i < bulutlar.Count; i++)
            {
                PointF b = bulutlar[i];
                b.X += 0.3f + i * 0.08f;
                if (b.X > ClientSize.Width + 120) b.X = -160;
                bulutlar[i] = b;
            }

            for (int i = efektler.Count - 1; i >= 0; i--)
            {
                efektler[i].Yas++;
                if (efektler[i].Yas > (efektler[i].Leke ? 75 : 45)) efektler.RemoveAt(i);
            }

            if (raketVurus > 0) raketVurus--;
            Invalidate();
        }

        // ---------------- Fare ----------------
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            fare = e.Location;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (durum != Durum.Oynuyor) return;
            raketVurus = 6;

            if (altinSinek != null && Isabet(altinSinek, e.Location))
            {
                skor += 3; vurus++;
                VurusEfekti(altinSinek, "+3", Color.FromArgb(255, 196, 0));
                altinSinek = null;
                altinSayac = rastgele.Next(200, 350);
            }
            else if (Isabet(sinek, e.Location))
            {
                skor++; vurus++;
                VurusEfekti(sinek, "+1", Color.White);
                sinek = YeniSinek(false);
            }
            else
            {
                iska++;
                efektler.Add(new Efekt { X = e.X, Y = e.Y - 30, Yazi = "Iska!", Renk = Color.FromArgb(255, 120, 120) });
            }
        }

        bool Isabet(Sinek s, Point p)
        {
            float dx = p.X - s.X, dy = p.Y - s.Y;
            return dx * dx + dy * dy < 36 * 36;   // raketin ağı geniş, biraz tolerans
        }

        void VurusEfekti(Sinek s, string yazi, Color renk)
        {
            efektler.Add(new Efekt { X = s.X, Y = s.Y, Leke = true, Renk = s.Altin ? Color.FromArgb(200, 150, 0) : Color.FromArgb(70, 60, 50) });
            efektler.Add(new Efekt { X = s.X, Y = s.Y - 30, Yazi = yazi, Renk = renk });
        }

        // ---------------- Çizim ----------------
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            ArkaPlanCiz(g);

            foreach (Efekt ef in efektler) if (ef.Leke) LekeCiz(g, ef);
            SinekCiz(g, sinek);
            if (altinSinek != null) SinekCiz(g, altinSinek);
            foreach (Efekt ef in efektler) if (!ef.Leke) YaziEfektiCiz(g, ef);

            UstBarCiz(g);

            if (durum == Durum.Giris) GirisCiz(g);
            if (durum == Durum.Bitti) SonucCiz(g);
            if (durum == Durum.Oynuyor) RaketCiz(g, fare.X, fare.Y);
        }

        void ArkaPlanCiz(Graphics g)
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            Rectangle gok = new Rectangle(0, 0, w, h);
            using (var b = new LinearGradientBrush(gok, Color.FromArgb(120, 200, 255), Color.FromArgb(225, 245, 255), 90f))
                g.FillRectangle(b, gok);

            // güneş ve ışıltısı
            for (int i = 4; i >= 1; i--)
                using (var b = new SolidBrush(Color.FromArgb(28, 255, 230, 120)))
                    g.FillEllipse(b, w - 170 - i * 18, UstBar + 30 - i * 18, 100 + i * 36, 100 + i * 36);
            using (var b = new SolidBrush(Color.FromArgb(255, 214, 70)))
                g.FillEllipse(b, w - 170, UstBar + 30, 100, 100);

            // bulutlar
            using (var b = new SolidBrush(Color.FromArgb(235, 255, 255, 255)))
                foreach (PointF p in bulutlar)
                {
                    g.FillEllipse(b, p.X, p.Y, 90, 40);
                    g.FillEllipse(b, p.X + 30, p.Y - 20, 70, 50);
                    g.FillEllipse(b, p.X + 70, p.Y - 4, 70, 40);
                }

            // tepeler (arkadaki açık, öndeki koyu)
            Tepe(g, h - 210, 70, 0.006f, Color.FromArgb(130, 200, 110));
            Tepe(g, h - 140, 50, 0.009f, Color.FromArgb(90, 170, 80));
            Tepe(g, h - 70, 30, 0.013f, Color.FromArgb(60, 140, 60));

            // çiçekler
            Color[] renkler = { Color.FromArgb(255, 90, 120), Color.FromArgb(255, 220, 60), Color.White, Color.FromArgb(180, 120, 255) };
            for (int i = 0; i < 26; i++)
            {
                int x = (i * 137 + 40) % w;
                int y = h - 55 + (i * 53) % 40;
                using (var sap = new Pen(Color.FromArgb(40, 110, 40), 2)) g.DrawLine(sap, x, y, x, y + 14);
                using (var b = new SolidBrush(renkler[i % renkler.Length]))
                    for (int k = 0; k < 5; k++)
                    {
                        double a = k * Math.PI * 2 / 5;
                        g.FillEllipse(b, x + (float)Math.Cos(a) * 5 - 4, y + (float)Math.Sin(a) * 5 - 4, 8, 8);
                    }
                g.FillEllipse(Brushes.Gold, x - 3, y - 3, 6, 6);
            }
        }

        void Tepe(Graphics g, int taban, int yukseklik, float sik, Color renk)
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            List<PointF> noktalar = new List<PointF>();
            noktalar.Add(new PointF(0, h));
            for (int x = 0; x <= w; x += 20)
                noktalar.Add(new PointF(x, taban - (float)Math.Sin(x * sik + taban) * yukseklik * 0.5f - yukseklik * 0.5f));
            noktalar.Add(new PointF(w, h));
            using (var b = new SolidBrush(renk)) g.FillPolygon(b, noktalar.ToArray());
        }

        // Sineği kendi yönüne döndürerek çizer
        void SinekCiz(Graphics g, Sinek s)
        {
            GraphicsState eski = g.Save();
            g.TranslateTransform(s.X, s.Y);

            // gölge (yerde)
            using (var b = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                g.FillEllipse(b, -12, 26, 24, 7);

            g.RotateTransform((float)(s.Aci * 180 / Math.PI));
            float olcek = s.Altin ? 1.25f : 1.1f;
            g.ScaleTransform(olcek, olcek);

            Color govde = s.Altin ? Color.FromArgb(230, 170, 20) : Color.FromArgb(45, 45, 52);
            Color koyu = s.Altin ? Color.FromArgb(170, 110, 0) : Color.FromArgb(20, 20, 24);

            // bacaklar
            using (var p = new Pen(koyu, 1.6f))
                for (int i = -1; i <= 1; i++)
                {
                    g.DrawLine(p, i * 5, 0, i * 7 - 2, -11);
                    g.DrawLine(p, i * 5, 0, i * 7 - 2, 11);
                }

            // altın sineğin parıltısı
            if (s.Altin)
                using (var b = new SolidBrush(Color.FromArgb(70 + (int)(40 * Math.Sin(s.Kanat * 0.3)), 255, 220, 80)))
                    g.FillEllipse(b, -28, -28, 56, 56);

            // kanatlar: hızlı çırpar (genişliği değişir)
            float acik = 0.55f + 0.45f * (float)Math.Abs(Math.Sin(s.Kanat));
            using (var b = new SolidBrush(Color.FromArgb(140, 210, 235, 255)))
            using (var p = new Pen(Color.FromArgb(160, 120, 150, 180), 1))
            {
                RectangleF ust = new RectangleF(-16, -6 - 18 * acik, 22, 18 * acik);
                RectangleF alt = new RectangleF(-16, 6, 22, 18 * acik);
                g.FillEllipse(b, ust); g.DrawEllipse(p, ust.X, ust.Y, ust.Width, ust.Height);
                g.FillEllipse(b, alt); g.DrawEllipse(p, alt.X, alt.Y, alt.Width, alt.Height);
            }

            // gövde ve çizgileri
            using (var b = new SolidBrush(govde)) g.FillEllipse(b, -15, -7, 24, 14);
            using (var p = new Pen(koyu, 1.5f))
            {
                g.DrawArc(p, -10, -7, 6, 14, 270, 180);
                g.DrawArc(p, -5, -7, 6, 14, 270, 180);
            }
            // baş ve kırmızı gözler
            using (var b = new SolidBrush(koyu)) g.FillEllipse(b, 6, -6, 12, 12);
            using (var b = new SolidBrush(Color.FromArgb(200, 30, 40)))
            {
                g.FillEllipse(b, 10, -8, 8, 8);
                g.FillEllipse(b, 10, 0, 8, 8);
            }
            using (var b = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                g.FillEllipse(b, 14, -7, 2.5f, 2.5f);
                g.FillEllipse(b, 14, 1, 2.5f, 2.5f);
            }

            g.Restore(eski);
        }

        void LekeCiz(Graphics g, Efekt ef)
        {
            int alfa = Math.Max(0, 200 - ef.Yas * 200 / 75);
            using (var b = new SolidBrush(Color.FromArgb(alfa, ef.Renk)))
            {
                g.FillEllipse(b, ef.X - 14, ef.Y - 12, 28, 24);
                for (int i = 0; i < 7; i++)
                {
                    double a = i * Math.PI * 2 / 7 + ef.X;
                    float r = 18 + (i % 3) * 5;
                    g.FillEllipse(b, ef.X + (float)Math.Cos(a) * r - 4, ef.Y + (float)Math.Sin(a) * r - 4, 8, 8);
                }
            }
        }

        void YaziEfektiCiz(Graphics g, Efekt ef)
        {
            int alfa = Math.Max(0, 255 - ef.Yas * 255 / 45);
            float y = ef.Y - ef.Yas * 1.2f;
            StringFormat orta = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var f = new Font("Segoe UI", 22, FontStyle.Bold))
            {
                using (var golge = new SolidBrush(Color.FromArgb(alfa / 2, 0, 0, 0)))
                    g.DrawString(ef.Yazi, f, golge, ef.X + 2, y + 2, orta);
                using (var b = new SolidBrush(Color.FromArgb(alfa, ef.Renk)))
                    g.DrawString(ef.Yazi, f, b, ef.X, y, orta);
            }
        }

        void UstBarCiz(Graphics g)
        {
            int w = ClientSize.Width;
            Rectangle r = new Rectangle(0, 0, w, UstBar);
            using (var b = new LinearGradientBrush(r, Color.FromArgb(230, 25, 35, 60), Color.FromArgb(230, 45, 30, 80), 0f))
                g.FillRectangle(b, r);

            using (var f = new Font("Segoe UI", 20, FontStyle.Bold))
                g.DrawString("SİNEK YAKALA", f, Brushes.White, 24, 18);

            // skor kutusu
            Kutu(g, new Rectangle(w / 2 - 190, 14, 170, 48), "SKOR", skor.ToString(), Color.FromArgb(120, 220, 120));
            Kutu(g, new Rectangle(w / 2 + 0, 14, 170, 48), "REKOR", rekor.ToString(), Color.FromArgb(255, 200, 60));

            // süre çubuğu
            Rectangle cubuk = new Rectangle(w - 300, 22, 270, 32);
            using (GraphicsPath p = Yuvarlak(cubuk, 16))
            using (var b = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                g.FillPath(b, p);
            float oran = sure / 30f;
            Color renk = sure > 10 ? Color.FromArgb(80, 200, 255) : (sure % 2 == 0 ? Color.FromArgb(255, 80, 80) : Color.FromArgb(255, 150, 80));
            int dolu = (int)(cubuk.Width * oran);
            if (dolu > 32)
                using (GraphicsPath p = Yuvarlak(new Rectangle(cubuk.X, cubuk.Y, dolu, cubuk.Height), 16))
                using (var b = new SolidBrush(renk))
                    g.FillPath(b, p);
            StringFormat orta = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var f = new Font("Segoe UI", 12, FontStyle.Bold))
                g.DrawString("Süre: " + sure + " sn", f, Brushes.White, cubuk, orta);
        }

        void Kutu(Graphics g, Rectangle r, string baslik, string deger, Color renk)
        {
            using (GraphicsPath p = Yuvarlak(r, 12))
            using (var b = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                g.FillPath(b, p);
            using (var f = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var b = new SolidBrush(Color.FromArgb(190, 200, 230)))
                g.DrawString(baslik, f, b, r.X + 14, r.Y + 15);
            StringFormat sag = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            using (var f = new Font("Segoe UI", 20, FontStyle.Bold))
            using (var b = new SolidBrush(renk))
                g.DrawString(deger, f, b, new Rectangle(r.X, r.Y, r.Width - 12, r.Height), sag);
        }

        void PencereCiz(Graphics g, Rectangle r)
        {
            using (var b = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                g.FillRectangle(b, 0, UstBar, ClientSize.Width, ClientSize.Height - UstBar);
            using (GraphicsPath p = Yuvarlak(r, 24))
            {
                using (var b = new LinearGradientBrush(r, Color.FromArgb(245, 45, 30, 80), Color.FromArgb(245, 25, 35, 60), 90f))
                    g.FillPath(b, p);
                using (var kenar = new Pen(Color.FromArgb(255, 200, 60), 4))
                    g.DrawPath(kenar, p);
            }
        }

        void GirisCiz(Graphics g)
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            Rectangle r = new Rectangle(w / 2 - 280, h / 2 - 190, 560, 360);
            PencereCiz(g, r);
            StringFormat orta = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var f = new Font("Segoe UI", 34, FontStyle.Bold))
            using (var b = new SolidBrush(Color.FromArgb(255, 200, 60)))
                g.DrawString("SİNEK YAKALA", f, b, new Rectangle(r.X, r.Y + 20, r.Width, 70), orta);
            using (var f = new Font("Segoe UI", 12))
            using (var b = new SolidBrush(Color.FromArgb(220, 225, 245)))
            {
                g.DrawString("30 saniyede raketle yakalayabildiğin kadar sinek yakala!", f, b, new Rectangle(r.X, r.Y + 96, r.Width, 30), orta);
                g.DrawString("Her sinek +1   •   Altın sinek +3   •   Skor arttıkça sinek hızlanır", f, b, new Rectangle(r.X, r.Y + 126, r.Width, 30), orta);
            }
            // örnek sinekler
            SinekCiz(g, new Sinek { X = w / 2 - 50, Y = r.Y + 190, Kanat = tik * 1.4f });
            SinekCiz(g, new Sinek { X = w / 2 + 50, Y = r.Y + 190, Kanat = tik * 1.4f + 1, Altin = true });
        }

        void SonucCiz(Graphics g)
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            Rectangle r = new Rectangle(w / 2 - 280, h / 2 - 190, 560, 360);
            PencereCiz(g, r);
            StringFormat orta = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var f = new Font("Segoe UI", 16, FontStyle.Bold))
            using (var b = new SolidBrush(Color.FromArgb(255, 200, 60)))
                g.DrawString("SÜRE DOLDU!", f, b, new Rectangle(r.X, r.Y + 20, r.Width, 36), orta);
            using (var f = new Font("Segoe UI", 54, FontStyle.Bold))
                g.DrawString(skor.ToString(), f, Brushes.White, new Rectangle(r.X, r.Y + 60, r.Width, 90), orta);
            using (var f = new Font("Segoe UI", 12))
            using (var b = new SolidBrush(Color.FromArgb(200, 210, 235)))
            {
                g.DrawString("puan", f, b, new Rectangle(r.X, r.Y + 145, r.Width, 24), orta);
                int toplam = vurus + iska;
                int isabet = toplam == 0 ? 0 : vurus * 100 / toplam;
                g.DrawString("Vuruş: " + vurus + "   •   Iska: " + iska + "   •   İsabet: %" + isabet, f, b, new Rectangle(r.X, r.Y + 180, r.Width, 26), orta);
            }
            string mesaj = (skor >= rekor && skor > 0) ? "Yeni rekor!" : "Rekor: " + rekor;
            using (var f = new Font("Segoe UI", 14, FontStyle.Bold))
            using (var b = new SolidBrush(Color.FromArgb(120, 220, 120)))
                g.DrawString(mesaj, f, b, new Rectangle(r.X, r.Y + 212, r.Width, 30), orta);
        }

        // Fare imlecinin yerine sinek raketi
        void RaketCiz(Graphics g, int x, int y)
        {
            GraphicsState eski = g.Save();
            g.TranslateTransform(x, y);
            g.RotateTransform(raketVurus > 0 ? -12 : 18);
            float olcek = raketVurus > 0 ? 0.88f : 1f;
            g.ScaleTransform(olcek, olcek);

            // sap
            using (var p = new Pen(Color.FromArgb(150, 90, 40), 7) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(p, 0, 30, 0, 110);
            using (var p = new Pen(Color.FromArgb(60, 60, 70), 9) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(p, 0, 85, 0, 112);

            // ağ
            RectangleF ag = new RectangleF(-30, -32, 60, 64);
            using (GraphicsPath p = Yuvarlak(Rectangle.Round(ag), 14))
            {
                using (var b = new SolidBrush(Color.FromArgb(70, 240, 80, 80)))
                    g.FillPath(b, p);
                g.SetClip(p, CombineMode.Intersect);
                using (var ince = new Pen(Color.FromArgb(150, 160, 30, 30), 1))
                    for (int i = -30; i <= 30; i += 8)
                    {
                        g.DrawLine(ince, i, -32, i, 32);
                        g.DrawLine(ince, -30, i, 30, i);
                    }
                g.ResetClip();
                using (var kenar = new Pen(Color.FromArgb(220, 50, 50), 4))
                    g.DrawPath(kenar, p);
            }
            g.Restore(eski);
        }

        GraphicsPath Yuvarlak(Rectangle r, int yaricap)
        {
            int d = yaricap * 2;
            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
