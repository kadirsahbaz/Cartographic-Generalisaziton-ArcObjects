using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using KadirSahbaz;

namespace Genellestirme
{
    public class BinaBasitlestir : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public BinaBasitlestir()
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

            gp.AddToolbox(@"C:\TEZ\MyTools.tbx");
            IVariantArray parametreler = new VarArrayClass();
            parametreler.Add("Bina_Buyut");
            parametreler.Add(@"C:\TEZ\TezGDB.gdb\Binalar_Bst");

            gp.Execute("BinaBasitlestir", parametreler, null);

            //GEN.IsmeGoreTabakaGetir("Bina_Buyut").Visible = false;
        }

        protected override void OnUpdate()
        {
        }
    }
}
