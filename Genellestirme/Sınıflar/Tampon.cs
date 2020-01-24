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

namespace KadirSahbaz
{
    class Tampon
    {
        #region Değişkenler
        private int id;
        private IPolygon shape;
        private double area;

        public int ID { get { return id; } set { id = value; } }
        public IPolygon Shape { get { return shape; } set { shape = value; } }
        public double Area { get { return area; } set { area = value; } }
        #endregion


        public Tampon() { }

        public Tampon(IFeature tamponF)
        {
            if (tamponF != null)
            {
                this.id = tamponF.OID;
                this.shape = tamponF.ShapeCopy as IPolygon;
                this.area = (this.Shape as IArea).Area;
            }
        }

    }
}
