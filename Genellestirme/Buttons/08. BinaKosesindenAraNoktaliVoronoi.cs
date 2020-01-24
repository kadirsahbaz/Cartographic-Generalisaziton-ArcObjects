using System;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using KadirSahbaz;





namespace Genellestirme
{
    public class BinaKosesindenAraNoktaliVoronoi : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        int limit;
        public BinaKosesindenAraNoktaliVoronoi()
        {
        }

        public BinaKosesindenAraNoktaliVoronoi(int limit)
        {
            this.limit = limit;
        }

        public void Calistir()
        {
            OnClick();
        }

        protected override void OnClick()
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IMap map = mxDoc.FocusMap;
            IActiveView activeView = mxDoc.ActiveView;
            ILayer layer = BinaGEN.Layer("Binalar_Yeni2");
            map.ClearSelection();

            try
            {
                //if (!(limit > 0))
                //{
                //    DialogResult dr = new DialogResult();
                //    InputForm limitForm = new InputForm();
                //    limitForm.StartPosition = FormStartPosition.CenterParent;
                //    limitForm.Text = "Minimum Bina Kenarı";
                //    limitForm.label1.Text = "Girilen değerden daha uzun bina kenarlarına ara\nnokta eklenecektir.";

                //    // Limit değeri girmek için input penceresi açılıyor
                //    dr = limitForm.ShowDialog();

                //    if (dr == DialogResult.Cancel || limitForm.textBox1.Text == "") return;
                //    limit = Convert.ToInt32(limitForm.textBox1.Text);
                //}

                limit = 10;
                // Voronoi oluşturuyoruz.
                BinaGEN.PoligondanVoronoiOlustur(layer, limit);
                //

                //Voronoi tabakası sembolojisini ayarlıyoruz.
                ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass
                {
                    Style = esriSimpleLineStyle.esriSLSSolid,
                    Color = new RgbColorClass { Red = 255, Green = 0, Blue = 0 }
                };

                ISimpleFillSymbol fillSymbol = new SimpleFillSymbolClass
                {
                    Style = esriSimpleFillStyle.esriSFSNull,
                    Outline = lineSymbol,
                    Color = new RgbColorClass { Red = 255, Green = 0, Blue = 0 }
                };

                ISimpleRenderer simpleRenderer = new SimpleRendererClass();
                simpleRenderer.Symbol = (ISymbol)fillSymbol;
                (BinaGEN.Layer("Binalar_Yeni2_N_Voronoi") as IGeoFeatureLayer).Renderer= (IFeatureRenderer)simpleRenderer ;
                
                //Ekstra işlemler
                mxDoc.UpdateContents();
                activeView.Refresh();
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj(hata.Source, hata.Message);
            }
        }

        protected override void OnUpdate()
        {
        }
    }
}