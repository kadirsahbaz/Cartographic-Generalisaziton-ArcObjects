#region Namspaces
using System;
using System.Linq;
using System.Collections.Generic;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
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
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GeoDatabaseDistributed;
using System.Runtime.InteropServices;
#endregion

namespace KadirSahbaz
{

    public class VoronoiBilgisi
    {
        int id;
        double yogunluk;
        int binaSayisi, kareBinaSayisi, dikdortgenBinaSayisi, konutSayisi, resmiBinaSayisi;
        double maxBinaAlani, minBinaAlani, binaToplamAlan, alan;
        IPolygon shape;
        GenellestirmeTuru uygulanacakGenellestirme;
        
        public int ID { get { return id; } set { id = value; } }
        public IPolygon Shape { get { return shape; } set { shape = value; } }
        public int ResmiBinaSayisi { get { return resmiBinaSayisi; } set { resmiBinaSayisi = value; } }
        public int KonutSayisi { get { return konutSayisi; } set { konutSayisi = value; } }
        public int DikdortgenBinaSayisi { get { return dikdortgenBinaSayisi; } set { dikdortgenBinaSayisi = value; } }
        public int KareBinaSayisi { get { return kareBinaSayisi; } set { kareBinaSayisi = value; } }
        public int BinaSayisi { get { return binaSayisi; } set { binaSayisi = value; } }
        public double Yogunluk { get { return yogunluk; } set { yogunluk = value; } }
        public double Alan { get { return alan; } set { alan = value; } }
        public double BinaToplamAlan { get { return binaToplamAlan; } set { binaToplamAlan = value; } }
        public double MinBinaAlani { get { return minBinaAlani; } set { minBinaAlani = value; } }
        public double MaxBinaAlani { get { return maxBinaAlani; } set { maxBinaAlani = value; } }
        internal GenellestirmeTuru UygulanacakGenellestirme
        {
            get { return uygulanacakGenellestirme; }
            set { uygulanacakGenellestirme = value; }
        }

        public VoronoiBilgisi(IFeature feature)
        {
            this.id = feature.OID;
            this.shape = feature.Shape as IPolygon;
        }

        public override string ToString()
        {
            string str;
            str = this.id.ToString() + ";" +
                  this.binaSayisi.ToString() + ";" +
                  this.kareBinaSayisi.ToString() + ";" +
                  this.dikdortgenBinaSayisi.ToString() + ";" +
                  this.konutSayisi.ToString() + ";" +
                  this.resmiBinaSayisi.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.minBinaAlani.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.maxBinaAlani.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.alan.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.binaToplamAlan.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.yogunluk.ToString(CultureInfo.InvariantCulture) + "%;" +
                  Enum.GetName(typeof(GenellestirmeTuru), (int)uygulanacakGenellestirme);
            return str;
        }

    }
}
