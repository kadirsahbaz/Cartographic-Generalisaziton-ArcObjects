#region Namespaces
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Linq;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geoprocessing;
using KadirSahbaz;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessor;
using Analiz = ESRI.ArcGIS.AnalysisTools;
using VeriYonetimi = ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Runtime.InteropServices;
using System.Globalization;
#endregion


namespace Genellestirme
{
    public class GenellestirmeTuruBelirle: ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public GenellestirmeTuruBelirle()
        {
        }

        public void Calistir()
        {
            OnClick();
        }

        protected override void OnClick()
        {

            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            //IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap; map.ClearSelection();
            ILayer genBolLayer_5m = BinaGEN.Layer("Genellestirme_Bolgesi_5m"),
                   genBolLayer = BinaGEN.Layer("Genellestirme_Bolgesi"),
                   binaLayer = BinaGEN.Layer("Binalar_Yeni2"),
                   tamponLayer;
            
            //Binalara 5 m tampon alan oluşturuyoruz.
            string tamponAdi = binaLayer.Name + "_Tampon";
            if (BinaGEN.TabakaVarMi(map, tamponAdi) == false)
            {
                Geoprocessor gp = new Geoprocessor();
                gp.AddOutputsToMap = true;
                gp.OverwriteOutput = true;
                Analiz.Buffer binaTampon = new Analiz.Buffer();
                binaTampon.in_features = binaLayer;
                binaTampon.buffer_distance_or_field = "5";
                binaTampon.out_feature_class = BinaGEN.TabakaYolunuAl(binaLayer) + "_Tampon";
                gp.Execute(binaTampon, null);
            }
            tamponLayer = BinaGEN.Layer(tamponAdi);

            BinaGEN.GB_GenellestirmeTuruBelirle(genBolLayer, genBolLayer_5m, binaLayer, tamponLayer);            

        }

        protected override void OnUpdate()
        {
        }
    }
}
