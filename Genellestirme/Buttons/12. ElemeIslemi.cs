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
    public class ElemeIslemi: ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public ElemeIslemi()
        {
        }

        public void Calistir()
        {
            OnClick();
        }

        protected override void OnClick()
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap; map.ClearSelection();
            ILayer gbLayer = BinaGEN.Layer("Genellestirme_Bolgesi"),
                   binaLayer = BinaGEN.Layer("Binalar_Yeni2");
            IFeatureClass gbFC = (gbLayer as IFeatureLayer).FeatureClass,
                          binaFC = (binaLayer as IFeatureLayer).FeatureClass;
            IFeatureCursor gbCursor, binaCursor;
            IFeature gbF, binaF;
            IFields fields = gbFC.Fields as IFields;
            int genTurFieldNo = fields.FindField("GENELLESTIRME");
            string genTur;

            try
            {
                gbCursor = gbFC.Search(null, true);
                gbF = gbCursor.NextFeature();
                while (gbF != null)
                {
                    genTur = gbF.get_Value(genTurFieldNo).ToString();
                    if (genTur == "ELEME")
                    {
                        binaCursor = BinaGEN.IctekiOgeleriGetir(gbF, binaFC);
                        binaF = binaCursor.NextFeature();
                        if (binaF != null)
                        {
                            binaFC.GetFeature(binaF.OID).Delete();

                            gbF.set_Value(genTurFieldNo, "ELENDİ");
                            gbF.Store();
                            gbF = gbCursor.NextFeature();
                            continue;
                        }
                    }
                    gbF = gbCursor.NextFeature();
                }
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj("Hata", hata.Message);
            }
        }

        protected override void OnUpdate()
        {
        }
    }
}
