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
    public class GenellestirmeBolgesi
    {

        #region Değişkenler

        private int id;
        private IPolygon shape;
        private double length;
        private double area;
        private int koseSayisi;
        private IPoint centroid;
        private GenellestirmeTuru uygulanacakGenellestirme;

        public int ID { get { return id; } set { id = value; } }
        public IPolygon Shape { get { return shape; } set { shape = value; } }
        public double Length { get { return length; } set { length = value; } }
        public double Area { get { return area; } set { area = value; } }
        public int KoseSayisi { get { return koseSayisi; } set { koseSayisi = value; } }
        public IPoint Centroid { get { return centroid; } set { centroid = value; } }
        internal GenellestirmeTuru UygulanacakGenellestirme { get { return uygulanacakGenellestirme; } set { uygulanacakGenellestirme = value; } }

        #endregion


        public GenellestirmeBolgesi() { }

        public GenellestirmeBolgesi(IFeature genBol)
        {
            if (genBol != null)
            {
                this.id = genBol.OID;
                this.shape = genBol.ShapeCopy as IPolygon;
                this.length = this.Shape.Length;
                this.area = (this.Shape as IArea).Area;
                this.centroid = (this.Shape as IArea).Centroid;
                this.koseSayisi = (this.shape as IPointCollection).PointCount;
            }
        }
    }
}
