using System;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using KadirSahbaz;

namespace Genellestirme
{
    public class YerlesimAlanindakiBinalariSil : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public YerlesimAlanindakiBinalariSil()
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
                parametreler.Add("Binalar_Bst");
                parametreler.Add("Yerlesim_Yogun");
                parametreler.Add(@"C:\TEZ\TezGDB.gdb\Binalar_Yeni");
                gp.Execute("YerlesimAlanindakiBinalariSil", parametreler, null);

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
