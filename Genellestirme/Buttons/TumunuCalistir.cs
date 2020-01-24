
using System.Diagnostics;
using System;
using KadirSahbaz;
namespace Genellestirme
{
    public class TumunuCalistir: ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public TumunuCalistir()
        {
        }

        protected override void OnClick()
        {
            try
            {
                Stopwatch SureHesapla = new Stopwatch();
                SureHesapla.Start();

                //Genelleştirme İşlemlerini sırasıyla otomatik olarak yapar.
                /*1*/
                BlokAdaOlustur komut1 = new BlokAdaOlustur(); komut1.Calistir(); komut1.Dispose();

                /*2*/
                BinaBuyut komut2 = new BinaBuyut(); komut2.Calistir(); komut2.Dispose();

                /*3*/
                BinaBasitlestir komut3 = new BinaBasitlestir(); komut3.Calistir(); komut3.Dispose();

                /*4*/
                YerlesimAlaniGenislet komut4 = new YerlesimAlaniGenislet(); komut4.Calistir(); komut4.Dispose();

                /*5*/
                YerlesimAlaniOlustur komut5 = new YerlesimAlaniOlustur(); komut5.Calistir(); komut5.Dispose();

                /*6*/
                YerlesimAlaniBasitlestir komut6 = new YerlesimAlaniBasitlestir(); komut6.Calistir(); komut6.Dispose();

                /*7*/
                BinaTamponOlustur komut7 = new BinaTamponOlustur(); komut7.Calistir(); komut7.Dispose();

                /*8*/
                BinaKosesindenAraNoktaliVoronoi komut8 = new BinaKosesindenAraNoktaliVoronoi(10); komut8.Calistir(); komut8.Dispose();

                /*9*/
                VoronoiBolgesiOlustur komut9 = new VoronoiBolgesiOlustur(); komut9.Calistir(); komut9.Dispose(); ;

                /*10*/
                GenBolgesiOlustur komut10 = new GenBolgesiOlustur(); komut10.Calistir(); komut10.Dispose();

                /*11*/
                GenellestirmeTuruBelirle komut11 = new GenellestirmeTuruBelirle(); komut11.Calistir(); komut11.Dispose();
                /*12*/
                ElemeIslemi komut12 = new ElemeIslemi(); komut12.Calistir(); komut12.Dispose();

                /*13*/
                GBBinaTipiklestir komut13 = new GBBinaTipiklestir(); komut13.Calistir(); komut13.Dispose();

                SureHesapla.Stop();
                TimeSpan HesaplananZaman = SureHesapla.Elapsed;
                string Sonuc = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                HesaplananZaman.Hours, HesaplananZaman.Minutes, HesaplananZaman.Seconds, HesaplananZaman.Milliseconds / 10);

                BinaGEN.Mesaj("Süre", Sonuc);
            }
            catch (Exception)
            {

            }
        }

        protected override void OnUpdate()
        {
        }
    }
}
