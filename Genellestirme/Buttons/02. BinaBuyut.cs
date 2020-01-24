using System;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geoprocessor;
using KadirSahbaz;


namespace Genellestirme
{
    public class BinaBuyut : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public BinaBuyut()
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
            ILayer layer;
            map.ClearSelection();

            try
            {
                Geoprocessor gp = new Geoprocessor();
                gp.AddOutputsToMap = true;
                gp.OverwriteOutput = true;

                CopyFeatures cf = new CopyFeatures();
                cf.in_features = BinaGEN.Layer("Binalar");
                cf.out_feature_class = @"C:\TEZ\TezGDB.gdb\Bina_Buyut";
                gp.Execute(cf, null);
                
                cf.out_feature_class = @"C:\TEZ\TezGDB.gdb\Binalar_Copy";
                gp.Execute(cf, null);


                layer = BinaGEN.Layer("Bina_Buyut");

                //Asıl komut
                BinaGEN.BinaOlceklendir(layer);
                //


                //Ekstra işlemler
                //map.MoveLayer(BinaGEN.Layer("Binalar"), 0);
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
