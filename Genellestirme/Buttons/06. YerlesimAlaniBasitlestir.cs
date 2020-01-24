using System;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using KadirSahbaz;


namespace Genellestirme
{
    public class YerlesimAlaniBasitlestir : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public YerlesimAlaniBasitlestir()
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
                IVariantArray parametreler = new VarArrayClass();
                parametreler = new VarArrayClass();
                parametreler.Add("Blok_Ada");
                parametreler.Add("Yerlesim_Yogun");
                parametreler.Add(@"C:\TEZ\TezGDB.gdb\Yerlesim_Yeni");
                gp.Execute("YerlesimAlaniBasitlestir", parametreler, null);
                map.MoveLayer(BinaGEN.Layer("Binalar_Yeni2"), 0);

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
