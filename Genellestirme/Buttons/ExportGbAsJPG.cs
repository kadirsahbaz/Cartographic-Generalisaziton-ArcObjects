#region Namspaces
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using System;
using System.Linq;
using System.Collections.Generic;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesGDB;
using Genellestirme;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using ESRI.ArcGIS.GeoDatabaseDistributed;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.CartographyTools;
using KadirSahbaz;
#endregion


namespace Genellestirme
{
    public class ExportGbAsJPG: ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public ExportGbAsJPG()
        {
        }

        protected override void OnClick()
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap; map.ClearSelection();
            ILayer genBolLayer = BinaGEN.Layer("Genellestirme_Bolgesi");
            IFeatureClass genBolFC = (genBolLayer as IFeatureLayer).FeatureClass;
            IFeatureCursor genBolCursor;
            IFeature genBolFeature;
            IFeatureLayer2 fLayer = genBolLayer as IFeatureLayer2;
            IFeatureSelection fSelection = fLayer as IFeatureSelection;
            IEnvelope env;
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            IQueryFilter qF = new QueryFilterClass();
            string dosyaAdi, dosyaYolu, b;
            int genFieldNo = genBolFC.FindField("GENELLESTIRME");
            int gbID;

            //Tipikleştirme Kontrolü
            genBolCursor = genBolFC.Search(null, true);
            genBolFeature = genBolCursor.NextFeature();
            while (genBolFeature != null)
            {
                gbID = genBolFeature.OID;
                b = genBolFeature.get_Value(genFieldNo).ToString();

                qF.WhereClause = "OBJECTID=" + gbID.ToString();
                fSelection.SelectFeatures(qF, esriSelectionResultEnum.esriSelectionResultNew, true);
                env = genBolFeature.Extent;
                env.Expand(2, 2, true);
                activeView.Extent = env;

                dosyaAdi = gbID.ToString() + "_" + b + ".jpg";
                string klasor = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Gen";
                if (!Directory.Exists(klasor)) Directory.CreateDirectory(klasor);
                dosyaYolu = klasor + "\\" + dosyaAdi;

                BinaGEN.FeatureJpgOnizlemeOlustur(activeView, dosyaYolu);


                genBolFeature = genBolCursor.NextFeature();
            }



            ////Genelleştirme türü kontrolü
            //genBolCursor = genBolFC.Search(null, true);
            //genBolFeature = genBolCursor.NextFeature();
            //while (genBolFeature != null)
            //{
            //    gbID = genBolFeature.OID;
            //    b = genBolFeature.get_Value(genFieldNo).ToString();

            //    qF.WhereClause = "OBJECTID=" + gbID.ToString();
            //    fSelection.SelectFeatures(qF, esriSelectionResultEnum.esriSelectionResultNew, true);
            //    env = genBolFeature.Extent;
            //    env.Expand(3, 3, true);
            //    activeView.Extent = env;

            //    dosyaAdi = gbID.ToString() + "_" + b + ".jpg";
            //    dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Gen\\" + dosyaAdi;

            //    BinaGEN.FeatureJpgOnizlemeOlustur(activeView, dosyaYolu);
            //    genBolFeature = genBolCursor.NextFeature();
            //}

        }

        protected override void OnUpdate()
        {
        }
    }
}
