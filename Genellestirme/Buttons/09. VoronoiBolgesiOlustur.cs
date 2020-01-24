using System;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using KadirSahbaz;


namespace Genellestirme
{
    public class VoronoiBolgesiOlustur : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public VoronoiBolgesiOlustur()
        {
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
            map.ClearSelection();

            IGeoProcessor2 gp = new GeoProcessorClass();
            gp.OverwriteOutput = true;
            gp.AddOutputsToMap = true;

            try
            {
                //Bina küme clip                
                gp.AddToolbox(@"C:\TEZ\MyTools.tbx");
                IVariantArray parametreler = new VarArrayClass();
                parametreler.Add("Blok_Ada");
                parametreler.Add("Bina_Tampon");
                parametreler.Add(@"C:\TEZ\Default.gdb\Bina_Tampon_Clip");
                gp.Execute("BinaTamponClip", parametreler, null);
                map.ClearSelection();

                //Voronoi clip
                parametreler = new VarArrayClass();
                parametreler.Add("Binalar_Yeni2_N_Voronoi");
                parametreler.Add(@"C:\TEZ\TezGDB.gdb\Voronoi_Line");
                gp.Execute("VoronoiClip", parametreler, null);
                map.ClearSelection();       

                //Voronoi to polygon
                parametreler = new VarArrayClass();
                parametreler.Add("Bina_Tampon_Clip");
                parametreler.Add("Voronoi_Line");
                parametreler.Add(@"C:\TEZ\TezGDB.gdb\Voronoi_5m");
                gp.Execute("Voronoi2Polygon", parametreler, null);
                map.ClearSelection();

                //Voronoi bölgesi
                gp.AddOutputsToMap = true;
                parametreler = new VarArrayClass();
                parametreler.Add("Bina_Tampon_Clip");
                parametreler.Add("Voronoi_5m");
                parametreler.Add(@"C:\TEZ\TezGDB.gdb\Voronoi_Bolgesi");
                gp.Execute("VoronoiBolgesi", parametreler, null);
                
                //Ekstra işlemler
                //map.DeleteLayer(BinaGEN.IsmeGoreTabakaGetir("Binalar_Yeni2_N_Voronoi"));
                //map.DeleteLayer(BinaGEN.IsmeGoreTabakaGetir("Bina_Tampon_Clip"));
                //map.DeleteLayer(BinaGEN.IsmeGoreTabakaGetir("Voronoi_Line"));
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
