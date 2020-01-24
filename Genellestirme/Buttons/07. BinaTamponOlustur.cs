using System;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using KadirSahbaz;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;


namespace Genellestirme
{
    public class BinaTamponOlustur : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public BinaTamponOlustur()
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
                parametreler.Add("Binalar_Yeni2");
                parametreler.Add(@"C:\TEZ\TezGDB.gdb\Bina_Tampon");
                gp.Execute("BinaTampon", parametreler, null);

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
