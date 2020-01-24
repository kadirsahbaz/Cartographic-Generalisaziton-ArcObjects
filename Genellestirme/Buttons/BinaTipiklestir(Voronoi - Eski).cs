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
    public class BinaTipiklestir : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public BinaTipiklestir()
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
            ILayer voronoiLayer = BinaGEN.Layer("Voronoi_Bolgesi"),
                   tamponLayer = BinaGEN.Layer("Bina_Tampon"),
                   binaLayer = BinaGEN.Layer("Binalar_Yeni2");

            BinaGEN.VoronoiAnalizEtVeTipiklestir(voronoiLayer, tamponLayer, binaLayer);

            map.ClearSelection();
            mxDoc.UpdateContents();
            activeView.Refresh();
        } /* OnClick Sonu */

        


        protected override void OnUpdate()
        {
        }
    }
}
