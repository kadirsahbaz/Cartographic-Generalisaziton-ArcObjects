using System;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using KadirSahbaz;


namespace Genellestirme
{
    public class GenBolgesiOlustur : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public GenBolgesiOlustur()
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
            gp.AddOutputsToMap = true;
            gp.OverwriteOutput = true;

            try
            {
                gp.AddToolbox(@"C:\TEZ\MyTools.tbx");
                IVariantArray parametreler = new VarArrayClass();

                parametreler = new VarArrayClass();
                parametreler.Add("Blok_Ada");
                parametreler.Add("Binalar_Yeni2");
                gp.Execute("BinalaraBlockNoYaz", parametreler, null);
                
                map.ClearSelection();
                parametreler = new VarArrayClass();
                parametreler.Add(@"C:\TEZ\TezGDB.gdb");
                gp.Execute("GenellestirmeFCOlustur", parametreler, null);

                map.ClearSelection();
                parametreler = new VarArrayClass();
                parametreler.Add("Blok_Ada");
                parametreler.Add("Binalar_Yeni2");
                parametreler.Add("Voronoi_Bolgesi");
                parametreler.Add("Voronoi_5m");
                parametreler.Add("Genellestirme_Bolgesi");
                parametreler.Add("Genellestirme_Bolgesi_5m");
                gp.Execute("GenellestirmeBolgesi", parametreler, null);

                //parametreler = new VarArrayClass();
                //parametreler.Add("Genellestirme_Bolgesi_5m");
                //parametreler.Add("Genellestirme_Bolgesi");
                //gp.Execute("GenellestirmeBolgesiTemizle", parametreler, null);



                //Ekstra işlemler
                map.ClearSelection();
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
