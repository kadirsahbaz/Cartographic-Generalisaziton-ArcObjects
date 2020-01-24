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
#endregion

namespace KadirSahbaz
{
    // KENDİ OLUŞTURDUĞUM FONKSİYONLARI İÇEREN SINIF

    enum GenellestirmeTuru
    {
        SEMANTIKSECME,
        SEMANTIKGRUPLANDIRMA,
        BUYULTME,
        ABARTMA,
        YUMUSATMA,
        FRAKTALLASTIRMA,
        DUZELTME,
        SECME,
        ELEME,
        OTELEME,
        KAYNASTIRMA,
        KOMBINASYON,
        TIPIKLESTIRME,
        NONE
    }

    public static class BinaGEN
    {

        /* KOD İÇERİSİNDE KULLANILACAK FONKSİYONLAR BURADA TANIMLANACAK */
        /* ------------------------------------------------------------ */

        // Uyarı ve hatalar için kullanılacak genel mesaj penceresi
        private static IMessageDialog mesajPenceresi = new MessageDialogClass();


        #region "Poligondan Voronoi Oluşturma"
        /// <summary>Poligon tabakasındaki öğelerin köşe noktalarını temel alan voronoi çokgeni oluşturur.</summary>
        /// <param name="layer">Voronoi çokgeni oluşturulacak tabaka.</param>
        /// <param name="fClassName">Voronoi çokgeninin kaydedileceği featureclass ismi.</param>
        public static void PoligondanVoronoiOlustur(ILayer layer)
        {
            ILayer layerNokta;
            layerNokta = KoseleriNoktayaCevir(layer);
            if (layerNokta == null)
            {
                BinaGEN.Mesaj("UYARI", "Poligon köşeleri noktaya çevrilemedi.");
                return;
            }
            VoronoiOlustur(layerNokta);
        }
        #endregion

        #region "Poligon Kenarlarına Ara Nokta Ekleyerek Voronoi Çokgeni Oluşturma"
        /// <summary>Poligon tabakasındaki öğelerin köşe noktalarını temel alan voronoi çokgeni oluşturur.</summary>
        /// <param name="layer">Voronoi çokgeni oluşturulacak tabaka.</param>
        /// <param name="fClassName">Voronoi çokgeninin kaydedileceği featureclass ismi.</param>
        /// <param name="limit">Poligon kenarının olabileceği azami uzunluk. Bu değerden yukarıda olan
        /// kenarlara gerekli sayıda nokta eklenir.</param>
        public static void PoligondanVoronoiOlustur(ILayer layer, int limit)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap;
            IFeature feature, noktaFeature;
            IPoint p1, p2, araNokta, ilkAraNokta;
            IPointCollection binaPColl;
            ILayer layerKontrol;
            IFeatureLayer fLayer = new FeatureLayerClass();
            IFeatureClass binaFClass, yeniFClass;
            IFeatureCursor binaFCursor;
            string fClassName = layer.Name + "_N";

            // Noktaları tutacak yeni bir featureclass oluşturuyoruz.
            string dosyaGDB = TabakaKlasoruAl(layer);
            IWorkspaceFactory wsf = new FileGDBWorkspaceFactoryClass();
            IWorkspace2 ws = (IWorkspace2)wsf.OpenFromFile(dosyaGDB, ArcMap.Application.hWnd);
            yeniFClass = YeniFeatureClassOlustur(ws, fClassName, map.SpatialReference, esriGeometryType.esriGeometryPoint, null);
            fLayer.Name = fClassName;
            fLayer.FeatureClass = yeniFClass;

            //Aynı isimde tabaka varsa onu kaldırıyoruz.
            IEnumLayer enumLayer = map.Layers;
            layerKontrol = enumLayer.Next();
            while (layerKontrol != null)
            {
                if (layerKontrol.Name == fClassName) map.DeleteLayer(layerKontrol);
                layerKontrol = enumLayer.Next();
            }

            binaFClass = (layer as IFeatureLayer).FeatureClass;

            int araNoktaAdedi;
            double kenar, aciklik;

            try
            {
                binaFCursor = binaFClass.Search(null, true);
                feature = binaFCursor.NextFeature();

                while (feature != null)
                {
                    //feature = binaFClass.GetFeature(i);
                    //GetFeature metodu objectID'ye göre çalışıyor, liste sırasına göre değil.
                    binaPColl = feature.Shape as IPointCollection;
                    for (int j = 0; j < binaPColl.PointCount - 1; j++)
                    {
                        p1 = binaPColl.get_Point(j);
                        //Binanın köşe noktalarını featureclass'a ekliyoruz.
                        noktaFeature = yeniFClass.CreateFeature();
                        noktaFeature.Shape = p1;
                        noktaFeature.Store();

                        if (binaPColl.get_Point(j + 1) != null)
                        {
                            p2 = binaPColl.get_Point(j + 1);
                            kenar = KenarUzunlugu(p1, p2);
                            aciklik = AciklikAcisi(p1, p2);
                            if (kenar > limit)
                            { // kenar uzunluğu, girdiğimiz değerden büyükse o kenara nokta(lar) atıyoruz.
                                araNoktaAdedi = (int)(kenar / limit);
                                double ilkMesafe = ((kenar - araNoktaAdedi * limit) + limit) / 2;
                                ilkAraNokta = new PointClass();
                                ilkAraNokta.Y = p1.Y + ilkMesafe * Math.Sin(aciklik * Math.PI / 180);
                                ilkAraNokta.X = p1.X + ilkMesafe * Math.Cos(aciklik * Math.PI / 180);
                                //İlk ara nokta featureclass'a ekleniyor.
                                noktaFeature = yeniFClass.CreateFeature();
                                noktaFeature.Shape = ilkAraNokta;
                                noktaFeature.Store();

                                for (int k = 1; k <= araNoktaAdedi - 1; k++)
                                {
                                    araNokta = new PointClass();
                                    araNokta.Y = ilkAraNokta.Y + k * limit * Math.Sin(aciklik * Math.PI / 180);
                                    araNokta.X = ilkAraNokta.X + k * limit * Math.Cos(aciklik * Math.PI / 180);
                                    noktaFeature = yeniFClass.CreateFeature();
                                    noktaFeature.Shape = araNokta;
                                    noktaFeature.Store();
                                }
                            }
                        }
                    }
                    feature = binaFCursor.NextFeature();
                } //Noktaları featureclass'a ekledik.
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj(hata.Source, hata.Message); return;
            }

            mxDoc.AddLayer(fLayer); //Oluşan featureclass'ı tabaka olarak ekle.
            activeView.Refresh();
            mxDoc.UpdateContents();

            VoronoiOlustur(fLayer);
        }
        #endregion

        #region "Poligon Merkezlerinden Voronoi Oluşturma"
        /// <summary> Poligon merkezini oluşturan noktalara ilişkin Voronoi
        /// Çokgenini oluşturur. </summary>
        /// <param name="layer">Merkez noktasından Voronoi Çokgeni oluşturulmak
        /// istenen poligon tabakası</param>
        public static void PoligonMerkezindenVoronoiOlustur(ILayer layer)
        {
            ILayer layerNokta;
            layerNokta = MerkezdenNoktaOlustur(layer);
            if (layerNokta == null)
            {
                BinaGEN.Mesaj("UYARI", "Poligon merkezleri noktaya çevrilemedi.");
                return;
            }
            VoronoiOlustur(layerNokta);
        }
        #endregion

        #region Poligon Üzerinde Ara Nokta Oluştur
        /// <summary>Poligon/Çizgi üzerinde -köşe noktalarla beraber- belirtilen aralıklarla nokta oluşturur </summary>        
        /// <param name="limit">İki nokta arasında olması istenen mesafe</param>
        public static void AraNoktaOlustur(ILayer layer, int limit)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap;
            IFeature feature, noktaFeature;
            IPoint p1, p2, araNokta, ilkAraNokta;
            IPointCollection binaPColl;
            ILayer layerKontrol;
            IFeatureLayer fLayer = new FeatureLayerClass();
            IFeatureClass binaFClass, yeniFClass;
            IFeatureCursor binaFCursor;
            string fClassName = layer.Name + "_N";

            // Noktaları tutacak yeni bir featureclass oluşturuyoruz.
            string dosyaGDB = TabakaKlasoruAl(layer);
            IWorkspaceFactory wsf = new FileGDBWorkspaceFactoryClass();
            IWorkspace2 ws = (IWorkspace2)wsf.OpenFromFile(dosyaGDB, ArcMap.Application.hWnd);
            yeniFClass = YeniFeatureClassOlustur(ws, fClassName, map.SpatialReference, esriGeometryType.esriGeometryPoint, null);
            fLayer.Name = fClassName;
            fLayer.FeatureClass = yeniFClass;

            #region Aynı isimde tabaka varsa onu kaldırıyoruz.
            IEnumLayer enumLayer = map.Layers;
            layerKontrol = enumLayer.Next();
            while (layerKontrol != null)
            {
                if (layerKontrol.Name == fClassName) map.DeleteLayer(layerKontrol);
                layerKontrol = enumLayer.Next();
            }
            #endregion

            binaFClass = (layer as IFeatureLayer).FeatureClass;

            int araNoktaAdedi;
            double kenar, aciklik;

            try
            {
                binaFCursor = binaFClass.Search(null, true);
                feature = binaFCursor.NextFeature();

                while (feature != null)
                {
                    //feature = binaFClass.GetFeature(i);
                    //GetFeature metodu objectID'ye göre çalışıyor, liste sırasına göre değil.
                    binaPColl = feature.Shape as IPointCollection;
                    for (int j = 0; j < binaPColl.PointCount - 1; j++)
                    {
                        p1 = binaPColl.get_Point(j);
                        //Binanın köşe noktalarını featureclass'a ekliyoruz.
                        noktaFeature = yeniFClass.CreateFeature();
                        noktaFeature.Shape = p1;
                        noktaFeature.Store();

                        if (binaPColl.get_Point(j + 1) != null)
                        {
                            p2 = binaPColl.get_Point(j + 1);
                            kenar = KenarUzunlugu(p1, p2);
                            aciklik = AciklikAcisi(p1, p2);
                            if (kenar > limit)
                            { // kenar uzunluğu, girdiğimiz değerden büyükse o kenara nokta(lar) atıyoruz.
                                araNoktaAdedi = (int)(kenar / limit);
                                double ilkMesafe = ((kenar - araNoktaAdedi * limit) + limit) / 2;
                                ilkAraNokta = new PointClass();
                                ilkAraNokta.Y = p1.Y + ilkMesafe * Math.Sin(aciklik * Math.PI / 180);
                                ilkAraNokta.X = p1.X + ilkMesafe * Math.Cos(aciklik * Math.PI / 180);
                                //İlk ara nokta featureclass'a ekleniyor.
                                noktaFeature = yeniFClass.CreateFeature();
                                noktaFeature.Shape = ilkAraNokta;
                                noktaFeature.Store();

                                for (int k = 1; k <= araNoktaAdedi - 1; k++)
                                {
                                    araNokta = new PointClass();
                                    araNokta.Y = ilkAraNokta.Y + k * limit * Math.Sin(aciklik * Math.PI / 180);
                                    araNokta.X = ilkAraNokta.X + k * limit * Math.Cos(aciklik * Math.PI / 180);
                                    noktaFeature = yeniFClass.CreateFeature();
                                    noktaFeature.Shape = araNokta;
                                    noktaFeature.Store();
                                }
                            }
                        }
                    }
                    feature = binaFCursor.NextFeature();
                } //Noktaları featureclass'a ekledik.
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj(hata.Source, hata.Message); return;
            }

            mxDoc.AddLayer(fLayer); //Oluşan featureclass'ı tabaka olarak ekle.
            activeView.Refresh();
            mxDoc.UpdateContents();
        }
        #endregion

        #region "Voronoi Oluşturma"
        /// <summary>Verilen nokta tabakasının voronoi çokgenini oluşturur.</summary>
        /// <param name="layer">Voronoi çokgeni oluşturulmak istenen nokta tabakası.</param>
        /// <param name="fClassName">Voronoi çokgenini içerecek olan yeni tabakaya verilmek istenen isim.</param>
        public static void VoronoiOlustur(ILayer layer)
        {
            Geoprocessor gp = new Geoprocessor();
            gp.AddOutputsToMap = true;
            gp.OverwriteOutput = true;

            CreateThiessenPolygons voronoi = new CreateThiessenPolygons();
            voronoi.in_features = layer;
            voronoi.fields_to_copy = "ALL";
            voronoi.out_feature_class = TabakaYolunuAl(layer) + "_Voronoi";

            gp.Execute(voronoi, null);
        }
        #endregion

        #region "Poligon Köşelerinden Nokta Tabakası Oluşturma"
        /// <summary>Bir poligon tabakasındaki öğelerin köşe noktalarından 
        /// yeni bir nokta tabakası oluşturur.</summary>
        /// <param name="layer">Poligon tabakası</param>
        /// <param name="fClassName"></param>
        /// <returns></returns>
        public static ILayer KoseleriNoktayaCevir(ILayer layer)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IMap map = mxDoc.FocusMap;
            IEnumLayer enumLayer = null;
            ILayer layerKontrol = null;
            string dosyaAdres, ilkDosyaAdres;
            ilkDosyaAdres = TabakaYolunuAl(layer) + "_N";

            Geoprocessor gp = new Geoprocessor();
            gp.AddOutputsToMap = true;
            gp.OverwriteOutput = true;

            FeatureVerticesToPoints noktalar = new FeatureVerticesToPoints();
            noktalar.in_features = layer;
            noktalar.point_location = "ALL";
            noktalar.out_feature_class = ilkDosyaAdres;

            gp.Execute(noktalar, null);

            //İşlem sonrası yeni eklenen tabakayı geri döndüreceğiz.
            enumLayer = map.get_Layers();
            layerKontrol = enumLayer.Next();
            dosyaAdres = TabakaYolunuAl(layerKontrol);
            while (layerKontrol != null)
            {
                if (dosyaAdres == ilkDosyaAdres) return layerKontrol;
                layerKontrol = enumLayer.Next();
            }

            return null;
        }
        #endregion

        #region "Poligon Merkezinden Nokta Tabakası Oluşturma"
        /// <summary>Poligon merkezlerinden oluşan nokta tabakası oluşturur. </summary>
        /// <param name="layer">Noktaya dönüştürülecek poligon tabakası</param>
        public static ILayer MerkezdenNoktaOlustur(ILayer layer)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IMap map = mxDoc.FocusMap;
            IEnumLayer enumLayer = null;
            ILayer layerKontrol = null;
            string dosyaAdres, ilkDosyaAdres;
            ilkDosyaAdres = TabakaYolunuAl(layer) + "_N";

            Geoprocessor gp = new Geoprocessor();
            gp.AddOutputsToMap = true;
            gp.OverwriteOutput = true;

            FeatureToPoint noktalar = new FeatureToPoint();
            noktalar.in_features = layer;
            noktalar.point_location = "CENTROID";
            noktalar.out_feature_class = ilkDosyaAdres;

            gp.Execute(noktalar, null);

            //İşlem sonrası yeni eklenen tabakayı geri döndüreceğiz.
            enumLayer = map.get_Layers();
            layerKontrol = enumLayer.Next();
            dosyaAdres = TabakaYolunuAl(layerKontrol);
            while (layerKontrol != null)
            {
                if (dosyaAdres == ilkDosyaAdres) return layerKontrol;
                layerKontrol = enumLayer.Next();
            }

            return null;
        }
        #endregion

        #region "Yeni FeatureClass Oluştur"
        /// <summary> Yeni feature class oluşturur. (Aynı isimdeki feature class'ı siler) </summary>
        /// <param name="ws"></param>
        /// <param name="fCName">Feature classa verilecek isim.</param>
        /// <param name="sRef">Referans(koordinat) sistemi.</param>
        /// <param name="type">Geometri türü.</param>
        /// <param name="fields">Eklenecek alanlar. 'null' girilirse ID ve Shape alanları otomatik eklenir.</param>
        public static IFeatureClass YeniFeatureClassOlustur(IWorkspace2 ws, String fCName, ISpatialReference sRef, esriGeometryType type, IFields fields)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IMap map = mxDoc.FocusMap;
            IFeatureClass featureClass;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)ws;
            string yeniTabakaIsmi = fCName;

            //Aynı isimde FeatureClass varsa yeni bir isim oluşturuyoruz.
            for (int i = 1; ; i++)
                if (ws.get_NameExists(esriDatasetType.esriDTFeatureClass, yeniTabakaIsmi))
                    yeniTabakaIsmi = fCName + "_" + i.ToString();
                else break;

            if (fields == null)
            {
                fields = new FieldsClass();
                IFieldsEdit fsEdit = (IFieldsEdit)fields;
                fsEdit.FieldCount_2 = 2;

                IFieldEdit field = new FieldClass();
                field.Name_2 = "ObjectID";
                field.Type_2 = esriFieldType.esriFieldTypeOID;
                fsEdit.Field_2[0] = field;

                IGeometryDefEdit geometryDef = new GeometryDefClass();
                geometryDef.GeometryType_2 = type;
                geometryDef.SpatialReference_2 = sRef;
                field = new FieldClass();
                field.Name_2 = "Shape";
                field.Type_2 = esriFieldType.esriFieldTypeGeometry;
                field.GeometryDef_2 = geometryDef;
                fsEdit.Field_2[1] = field;
            }
            featureClass = featureWorkspace.CreateFeatureClass(yeniTabakaIsmi, fields, null, null, esriFeatureType.esriFTSimple, "Shape", null);

            return featureClass;
        }
        #endregion

        #region "Bina Ölçeklendirme"
        /// <summary>Tabakanın belirtilen isimdeki alanın değeri oranında
        /// tabakayı ölçeklendirir.</summary>
        /// <param name="layer">Ölçeklendirilecek tabaka</param>
        /// <param name="olcekAlanAdi">Ölçeklendirme katsayısını içeren alan (field) adı.</param>
        public static void BinaOlceklendir(ILayer layer)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap;
            IFeature feature; IPoint origin; IArea bina;
            IFeatureLayer fLayer = new FeatureLayerClass();
            IFeatureClass binaFClass; IFeatureCursor binaFCursor;
            double olcek;

            try
            {
                binaFClass = (layer as IFeatureLayer).FeatureClass;
                binaFCursor = binaFClass.Search(null, true);
                feature = binaFCursor.NextFeature();

                while (feature != null)
                {
                    //Her bir binayı ölçeklendiriyoruz.
                    bina = feature.Shape as IArea;
                    origin = bina.Centroid;
                    //olcek = Convert.ToDouble(feature.get_Value(olcekFieldIndex));
                    olcek = Math.Sqrt((bina.Area + 469) / bina.Area);
                    //olcekY = Convert.ToDouble(feature.get_Value(olcekFieldIndex));
                    (feature.Shape as ITransform2D).Scale(origin, olcek, olcek);

                    feature.Store();
                    feature = binaFCursor.NextFeature();
                } //Noktaları featureclass'a ekledik.
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj(hata.Source, hata.Message); return;
            }

            mxDoc.AddLayer(fLayer); //Oluşan featureclass'ı tabaka olarak ekle.
            activeView.Refresh();
            mxDoc.UpdateContents();

        }
        #endregion

        #region "Kenar Uzunluğu Hesabı"
        /// <summary>İki nokta arasındaki mesafeyi verir.</summary>
        /// <param name="p1">İlk nokta</param>
        /// <param name="p2">İkinci nokta</param>
        /// <returns></returns>
        public static double KenarUzunlugu(IPoint p1, IPoint p2)
        {
            double x = p2.X - p1.X;
            double y = p2.Y - p1.Y;
            return Math.Sqrt(x * x + y * y);
        }
        #endregion

        #region "Açıklık Açısı Hesabı"
        /// <summary>İki nokta arasındaki açıklık açısını verir.</summary>
        /// <param name="p1">İlk nokta</param>
        /// <param name="p2">İkinci nokta</param>
        public static double AciklikAcisi(IPoint p1, IPoint p2)
        {
            double dX = p2.X - p1.X;
            double dY = p2.Y - p1.Y;
            double t;

            if (Math.Abs(dX) <= 1E-7) //payda sıfır ise
            {
                if (p2.Y > p1.Y) t = 90;
                else t = 270;
            }
            else
            {
                t = Math.Atan(dY / dX) * 180 / Math.PI;
                if (dX < 0) t = t + 180;
                if (t < 0) t = t + 360;
            }
            return t;
        }
        #endregion

        #region "Tabakanın Koordinat Sistemi Bilgilerini Öğrenme"
        ///<summary>Bir tabakanın kullandığı koordinat sistemini (spatial reference) döndürür.</summary>
        ///<param name="layer">Koordinat sistemi öğrenilmek istenen tabaka.</param>
        public static ISpatialReference KoordinatSistemiAl(ILayer layer)
        {
            if (layer is IGeoDataset)
            {
                IGeoDataset geoDataset = (IGeoDataset)layer;
                return geoDataset.SpatialReference;
            }
            else return null;
        }
        #endregion

        #region "Tabakanın Tam Yolunu Alma"
        ///<summary>Bir tabakanın dosya ismi de dahil olarak tam yolunu döndürür.</summary>
        ///<param name="layer">Tam adresi istenen tabaka değişkeni</param>
        public static System.String TabakaYolunuAl(ILayer layer)
        {
            if (layer == null || !(layer is IDataset))
            {
                return null;
            }
            IDataset dataset = (IDataset)(layer);

            return (dataset.Workspace.PathName + "\\" + dataset.Name);
        }
        #endregion

        #region "Tabakanın Bulunduğu Klasörü Öğrenme"
        ///<summary>Bir tabakadaki datasetin bulunduğu klasör yolunu döndürür.</summary>
        ///<param name="layer">Tam adresi istenen tabaka değişkeni</param>
        public static System.String TabakaKlasoruAl(ILayer layer)
        {
            if (layer == null || !(layer is IDataset))
            {
                return null;
            }
            IDataset dataset = (IDataset)(layer);

            return (dataset.Workspace.PathName);
        }
        #endregion

        #region "İsme Göre Tabakaya Ulaşma"
        /// <summary>İsmi verilen tabakayı döndürür.</summary>
        /// <param name="layerName">Tabakanın ismi</param>
        public static ILayer Layer(String layerName)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IMap map = mxDoc.FocusMap;

            int tabakaSayisi = map.LayerCount;

            for (Int32 i = 0; i < tabakaSayisi; i++)
                if (layerName == map.get_Layer(i).Name) return map.get_Layer(i);
            BinaGEN.Mesaj("Hata", layerName + " tabakası bulunamadı.");
            return null;
        }
        #endregion

        #region "İndex Numarasına Göre Tabakaya Ulaşma"
        /// <summary>İndex numarası verilen tabakayı döndürür.</summary>
        /// <param name="layerName">Tabakanın index numarası</param>
        public static ILayer Layer(int index)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IMap map = mxDoc.FocusMap;

            int tabakaSayisi = map.LayerCount;

            if (index >= tabakaSayisi)
            {
                BinaGEN.Mesaj("Hata", "Tabaka sayısından büyük bir değer girdiniz.");
                return null;
            }
            return map.get_Layer(index);
        }
        #endregion

        #region "Mesaj Göster"
        public static void Mesaj(string baslik, string metin)
        {
            mesajPenceresi.DoModal(baslik, metin, null, null, ArcMap.Application.hWnd);
        }
        #endregion

        #region Tabaka İsimlerini Al
        /// <summary>Tabakaları listeler.</summary>       
        public static List<string> TabakalariGetir(IMap map)
        {
            List<string> tabakalar = new List<string>();
            ILayer layer = null;
            IEnumLayer enumLayer = map.Layers;
            layer = enumLayer.Next();
            while (layer != null)
            {
                if (!(layer is IGroupLayer)) tabakalar.Add(layer.Name);
                layer = enumLayer.Next();
            }

            return tabakalar;
        }
        #endregion

        /// <summary>
        /// Belirtilen isimde tabaka olup olmadığı bildirir. 
        /// Tabakalara isim verilecek zaman kullanılabilir.
        /// </summary>
        public static bool TabakaVarMi(IMap map, string tabakaAdi)
        {
            ILayer layer = null;
            IEnumLayer enumLayer = map.Layers;
            layer = enumLayer.Next();
            while (layer != null)
            {
                if (layer.Name == tabakaAdi) return true;
                layer = enumLayer.Next();
            }
            return false;
        }

        /// <summary>Belirtilen türdeki tabakaları listeler.</summary>
        public static List<string> TabakalariGetir(IMap map, esriGeometryType geometry)
        {
            List<string> tabakalar = new List<string>();
            ILayer layer = null;
            IFeatureClass fClass = null;
            IEnumLayer enumLayer = map.Layers;
            layer = enumLayer.Next();
            while (layer != null)
            {
                if (!(layer is IGroupLayer) && (layer is IFeatureLayer))
                    fClass = (layer as IFeatureLayer).FeatureClass;
                {
                    if (fClass.ShapeType == geometry) tabakalar.Add(layer.Name);
                }

                layer = enumLayer.Next();
            }

            return tabakalar;
        }

        #region Bir Feature İçine Düşen Diğer Feature Sayısı
        /// <summary>Bir feature içine düşen diğer feature sayısını verir.</summary>
        /// <param name="kapsayanF">Kapsayan öğe</param>
        /// <param name="kapsananFC">Sayısı öğrenilecek, içerilen öğenin feature classı</param>
        public static int IctekiOgeSayisi(IFeature kapsayanF, IFeatureClass kapsananFC)
        {
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            ISelectionSet selection;

            spatialFilter.Geometry = kapsayanF.Shape;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            selection = kapsananFC.Select(spatialFilter, esriSelectionType.esriSelectionTypeSnapshot,
                        esriSelectionOption.esriSelectionOptionNormal, null);

            return selection.Count;
        }
        #endregion

        #region Bir Geometri İçine Düşen Diğer Feature Sayısı
        /// <summary>Bir geometri içine düşen diğer feature sayısını verir.</summary>
        /// <param name="kapsayanF">Kapsayan öğe</param>
        /// <param name="kapsananFC">Sayısı öğrenilecek, içerilen öğenin feature classı</param>
        public static int IctekiOgeSayisi(IGeometry kapsayanG, IFeatureClass kapsananFC)
        {
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            ISelectionSet selection;

            spatialFilter.Geometry = kapsayanG;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            selection = kapsananFC.Select(spatialFilter, esriSelectionType.esriSelectionTypeSnapshot,
                        esriSelectionOption.esriSelectionOptionNormal, null);

            return selection.Count;
        }
        #endregion

        #region Feature İçine Düşen Diğer Featurelar İçin Cursor Oluşturma
        /// <summary>Bir feature içine düşen diğer featurelar için cursor oluşturur.</summary>
        /// <param name="kapsayan">Kapsayan öğe</param>
        /// <param name="kapsanan">İçerilen öğenin feature classı</param>
        public static IFeatureCursor IctekiOgeleriGetir(IFeature kapsayanF, IFeatureClass kapsananFC)
        {
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            IFeatureCursor fCursor;

            spatialFilter.Geometry = kapsayanF.Shape;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            fCursor = kapsananFC.Search(spatialFilter, true);

            return fCursor;
        }
        #endregion

        #region Bir Geometri İçine Düşen Diğer Featurelar İçin Cursor Oluşturma
        /// <summary>Bir geometri içine düşen diğer featurelar için cursor oluşturur.</summary>
        /// <param name="kapsayan">Kapsayan öğe geometrisi</param>
        /// <param name="kapsanan">İçerilen öğenin feature classı</param>
        public static IFeatureCursor IctekiOgeleriGetir(IGeometry kapsayanG, IFeatureClass kapsananFC)
        {
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            IFeatureCursor fCursor;

            spatialFilter.Geometry = kapsayanG;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            fCursor = kapsananFC.Search(spatialFilter, true);

            return fCursor;
        }
        #endregion

        /*
        #region Shape İçine Düşen Diğer Feature İçin Cursor Oluşturma
        /// <summary>Bir shape içine düşen diğer featurelar için cursor oluşturur.</summary>
        /// <param name="kapsayan">Kapsayan öğe (shape)</param>
        /// <param name="kapsanan">İçerilen öğenin feature classı</param>
        public static IFeatureCursor IctekiOgeleriGetir(IGeometry kapsayanF, IFeatureClass kapsananFC)
        {
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            IFeatureCursor fCursor;

            spatialFilter.Geometry = kapsayanF as IGeometry;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            fCursor = kapsananFC.Search(spatialFilter, true);

            return fCursor;
        }
        #endregion
        */

        #region En Kısa Mesafe
        ///<summary>İki feature arasındaki uzaklığı (en kısa mesafeyi) bulur.</summary>
        public static double EnKisaMesafe(IFeature feature1, IFeature feature2)
        {
            IProximityOperator mesafeAnalizi = feature1.Shape as IProximityOperator;
            double mesafe = mesafeAnalizi.ReturnDistance(feature2.Shape as IGeometry);

            return mesafe;
        }
        #endregion

        #region ......Extension Metodlar......
        public static string BinaNumaralariGetir(this List<Bina> binalar)
        {
            string str = "";
            foreach (Bina b in binalar)
            {
                str += b.BinaID.ToString() + " ";
            }
            return str;
        }

        public static Bina GetByID(this List<Bina> binalar, int bID)
        {
            Bina bina = binalar.Where(k => k.BinaID == bID).ToList<Bina>()[0];
            return bina;
        }

        public static List<BinaMesafe> MesafeTablosuOlustur(this List<Bina> binalar)
        {
            List<BinaMesafe> binaMesafeler = new List<BinaMesafe>();

            foreach (Bina bina1 in binalar)
                foreach (Bina bina2 in binalar)
                    if (bina1.BinaID < bina2.BinaID)
                        binaMesafeler.Add(new BinaMesafe(bina1, bina2));

            return binaMesafeler;
        }

        #endregion

        //-------------------------------------------------------------------------------------
        //                      ......DOSYALAMA FONKSİYONLARI......

        #region Bina CSVKaydet
        /// <summary>Bina numaralarını belirtilen CSV dosyasına kaydeder.</summary>
        /// <param name="dosyaAdi">Dosya adı. (Masaüstüne kaydeder.)</param>
        public static void CSVKaydet(this List<Bina> binalar, string dosyaAdi)
        {
            string dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + ".csv";

            //Dosya zaten varsa, numaralandırıp yeni isim oluşturuyoruz.
            for (int i = 1; i < 1000; i++)
                if (File.Exists(dosyaYolu))
                    dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + "_" + i.ToString() + ".csv";
                else break;


            StreamWriter dosya = new StreamWriter(dosyaYolu, false, Encoding.UTF8);
            string baslik = "Bina ID";
            dosya.WriteLine(baslik);

            for (int i = 0; i < binalar.Count; i++)
                dosya.WriteLine(binalar[i].BinaID);

            dosya.Flush();
            dosya.Close();

        }
        #endregion

        #region BinaMesafe CSVKaydet
        /// <summary>Bina mesafelerini belirtilen CSV dosyasına kaydeder.</summary>
        /// <param name="dosyaAdi">Dosya adı. (Masaüstüne kaydeder.)</param>
        public static void CSVKaydet(this List<BinaMesafe> binaTablo, string dosyaAdi)
        {
            string dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + ".csv";

            //Dosya zaten varsa, numaralandırıp yeni isim oluşturuyoruz.
            for (int i = 1; ; i++)
                if (File.Exists(dosyaYolu))
                    dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + "_" + i.ToString() + ".csv";
                else break;


            StreamWriter dosya = new StreamWriter(dosyaYolu, false, Encoding.UTF8);
            int oncekiVorNo, oncekiKumeNo;
            string baslik = ";;" + "Bina1 ID;Bina2 ID;Mesafe";

            oncekiVorNo = binaTablo[0].VoronoiID;
            oncekiKumeNo = binaTablo[0].KumeID;
            dosya.WriteLine("Voronoi ID: ;" + binaTablo[0].VoronoiID);
            dosya.WriteLine(";Küme ID: ;" + binaTablo[0].KumeID);
            dosya.WriteLine(baslik);
            dosya.WriteLine(";;" + binaTablo[0].ToString());

            for (int i = 1; i < binaTablo.Count; i++)
            {
                if (binaTablo[i].VoronoiID == oncekiVorNo)
                {
                    if (binaTablo[i].KumeID == oncekiKumeNo)
                        dosya.WriteLine(";;" + binaTablo[i].ToString());
                    else
                    {
                        dosya.WriteLine();
                        dosya.WriteLine(";Küme ID: ;" + binaTablo[i].KumeID);
                        dosya.WriteLine(baslik);
                        dosya.WriteLine(";;" + binaTablo[i].ToString());
                        oncekiKumeNo = binaTablo[i].KumeID;
                    }
                }
                else
                {
                    dosya.WriteLine("Voronoi ID: ;" + binaTablo[i].VoronoiID);
                    dosya.WriteLine(";Küme ID: ;" + binaTablo[i].KumeID);
                    dosya.WriteLine(baslik);
                    dosya.WriteLine(";;" + binaTablo[i].ToString());
                    oncekiKumeNo = binaTablo[i].KumeID;
                    oncekiVorNo = binaTablo[i].VoronoiID;
                }
            }

            dosya.Flush();
            dosya.Close();

        }
        #endregion

        #region BinaKumeVoronoi CSVKaydet
        /// <summary>Bina-küme-voronoi ilişki tablosunu belirtilen CSV dosyasına kaydeder.</summary>
        /// <param name="dosyaAdi">Dosya adı. (Masaüstüne kaydeder.)</param>
        public static void CSVKaydet(this List<BinaKumeVoronoi> binaKumeVoronoiTablo, string dosyaAdi)
        {
            string dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + ".csv";
            for (int i = 1; ; i++)
                if (File.Exists(dosyaYolu))
                    dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + "_" + i.ToString() + ".csv";
                else break;


            StreamWriter dosya = new StreamWriter(dosyaYolu, false, Encoding.UTF8);
            int oncekiVorNo, oncekiKumeNo;
            string baslik = ";;" + "Bina ID;Uzunluk;Alan;Tür;Kompaktlık;Konvekslik;Uzanım;Dönüklük;Dikdörgensellik;Granülarite";

            oncekiVorNo = binaKumeVoronoiTablo[0].VoronoiID;
            oncekiKumeNo = binaKumeVoronoiTablo[0].KumeID;
            dosya.WriteLine("Voronoi ID: ;" + binaKumeVoronoiTablo[0].VoronoiID);
            dosya.WriteLine(";Küme ID: ;" + binaKumeVoronoiTablo[0].KumeID);
            dosya.WriteLine(baslik);
            dosya.WriteLine(";;" + binaKumeVoronoiTablo[0].ToString());

            for (int i = 1; i < binaKumeVoronoiTablo.Count; i++)
            {
                if (binaKumeVoronoiTablo[i].VoronoiID == oncekiVorNo)
                {
                    if (binaKumeVoronoiTablo[i].KumeID == oncekiKumeNo)
                        dosya.WriteLine(";;" + binaKumeVoronoiTablo[i].ToString());
                    else
                    {
                        dosya.WriteLine();
                        dosya.WriteLine(";Küme ID: ;" + binaKumeVoronoiTablo[i].KumeID);
                        dosya.WriteLine(baslik);
                        dosya.WriteLine(";;" + binaKumeVoronoiTablo[i].ToString());
                        oncekiKumeNo = binaKumeVoronoiTablo[i].KumeID;
                    }
                }
                else
                {
                    dosya.WriteLine("Voronoi ID: ;" + binaKumeVoronoiTablo[i].VoronoiID);
                    dosya.WriteLine(";Küme ID: ;" + binaKumeVoronoiTablo[i].KumeID);
                    dosya.WriteLine(baslik);
                    dosya.WriteLine(";;" + binaKumeVoronoiTablo[i].ToString());
                    oncekiKumeNo = binaKumeVoronoiTablo[i].KumeID;
                    oncekiVorNo = binaKumeVoronoiTablo[i].VoronoiID;
                }
            }

            dosya.Flush();
            dosya.Close();
        }
        #endregion

        #region BinaMesafe XMLKaydet
        /// <summary>Bina mesafelerini belirtilen XML dosyasına kaydeder.</summary>
        /// <param name="dosyaAdi">Dosya adı. (Masaüstüne kaydeder.)</param>
        public static void XMLKaydet(this List<BinaMesafe> binaTablo, string dosyaAdi)
        {
            string dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + ".xml";
            int oncekiVorNo, oncekiKumeNo;

            //Dosya zaten varsa, numaralandırıp yeni isim oluşturuyoruz.
            for (int i = 1; ; i++)
                if (File.Exists(dosyaYolu))
                    dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + "_" + i.ToString() + ".xml";
                else break;


            XElement xMesafe = null,
                xKume = new XElement("Kume", new XAttribute("id", binaTablo[0].KumeID)),
                xVoronoi = new XElement("Voronoi", new XAttribute("id", binaTablo[0].VoronoiID)),
                xBinaMesafe = new XElement("BinaMesafe");

            oncekiVorNo = binaTablo[0].VoronoiID;
            oncekiKumeNo = binaTablo[0].KumeID;
            for (int i = 0; i < binaTablo.Count; i++)
            {
                xMesafe = new XElement("MesafeKayit",
                    new XElement("Bina1ID", binaTablo[i].Bina1_ID),
                    new XElement("Bina2ID", binaTablo[i].Bina2_ID),
                    new XElement("Mesafe", binaTablo[i].Mesafe));

                if (binaTablo[i].VoronoiID == oncekiVorNo)
                {
                    if (binaTablo[i].KumeID == oncekiKumeNo)
                        xKume.Add(xMesafe);
                    else
                    {
                        xVoronoi.Add(xKume);
                        xKume = new XElement("Kume", new XAttribute("id", binaTablo[i].KumeID));
                        xKume.Add(xMesafe);
                        oncekiKumeNo = binaTablo[i].KumeID;
                    }
                }
                else
                {
                    xVoronoi.Add(xKume);
                    xBinaMesafe.Add(xVoronoi);
                    xKume = new XElement("Kume", new XAttribute("id", binaTablo[i].KumeID));
                    xVoronoi = new XElement("Voronoi", new XAttribute("id", binaTablo[i].VoronoiID));
                    xKume.Add(xMesafe);
                    oncekiVorNo = binaTablo[i].VoronoiID;
                    oncekiKumeNo = binaTablo[i].KumeID;
                }
            }

            XDocument xmlDosya = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                xBinaMesafe);
            xmlDosya.Save(dosyaYolu);
        }
        #endregion

        #region BinaKumeVoronoi XMLKaydet
        /// <summary>Bina-küme-voronoi ilişki tablosunu belirtilen CSV dosyasına kaydeder.</summary>
        /// <param name="dosyaAdi">Dosya adı. (Masaüstüne kaydeder.)</param>
        public static void XMLKaydet(this List<BinaKumeVoronoi> binaKumeVoronoiTablo, string dosyaAdi)
        {
            string dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + ".xml";
            int oncekiVorNo, oncekiKumeNo;

            //Dosya zaten varsa, numaralandırıp yeni isim oluşturuyoruz.
            for (int i = 1; ; i++)
                if (File.Exists(dosyaYolu))
                    dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + "_" + i.ToString() + ".xml";
                else break;


            XElement xBina = null,
                xKume = new XElement("Kume", new XAttribute("id", binaKumeVoronoiTablo[0].KumeID)),
                xVoronoi = new XElement("Voronoi", new XAttribute("id", binaKumeVoronoiTablo[0].VoronoiID)),
                xBinaKumeVoronoi = new XElement("BinaKumeVoronoi");

            oncekiVorNo = binaKumeVoronoiTablo[0].VoronoiID;
            oncekiKumeNo = binaKumeVoronoiTablo[0].KumeID;
            for (int i = 0; i < binaKumeVoronoiTablo.Count; i++)
            {
                xBina =
                    new XElement("Bina",
                        new XElement("binaID", binaKumeVoronoiTablo[i].BinaID),
                        new XElement("Uzunluk", binaKumeVoronoiTablo[i].Length),
                        new XElement("Alan", binaKumeVoronoiTablo[i].Area),
                        new XElement("Tur", binaKumeVoronoiTablo[i].Type),
                        new XElement("Kompaktlik", binaKumeVoronoiTablo[i].Compactness),
                        new XElement("Konvekslik", binaKumeVoronoiTablo[i].Convexity),
                        new XElement("Uzanim", binaKumeVoronoiTablo[i].Elongation),
                        new XElement("Donukluk", binaKumeVoronoiTablo[i].Orientation),
                        new XElement("Dikdortgensellik", binaKumeVoronoiTablo[i].Rectangularity),
                        new XElement("Granularite", binaKumeVoronoiTablo[i].Granularity));

                if (binaKumeVoronoiTablo[i].VoronoiID == oncekiVorNo)
                {
                    if (binaKumeVoronoiTablo[i].KumeID == oncekiKumeNo)
                        xKume.Add(xBina);
                    else
                    {
                        xVoronoi.Add(xKume);
                        xKume = new XElement("Kume", new XAttribute("id", binaKumeVoronoiTablo[i].KumeID));
                        xKume.Add(xBina);
                        oncekiKumeNo = binaKumeVoronoiTablo[i].KumeID;
                    }
                }
                else
                {
                    xVoronoi.Add(xKume);
                    xBinaKumeVoronoi.Add(xVoronoi);
                    xKume = new XElement("Kume", new XAttribute("id", binaKumeVoronoiTablo[i].KumeID));
                    xVoronoi = new XElement("Voronoi", new XAttribute("id", binaKumeVoronoiTablo[i].VoronoiID));
                    xKume.Add(xBina);
                    oncekiVorNo = binaKumeVoronoiTablo[i].VoronoiID;
                    oncekiKumeNo = binaKumeVoronoiTablo[i].KumeID;
                }
            }

            XDocument xmlDosya = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                xBinaKumeVoronoi);
            xmlDosya.Save(dosyaYolu);
        }
        #endregion

        #region Voronoi CSVKaydet
        /// <summary>Voronoi bilgileri tablosunu belirtilen CSV dosyasına kaydeder.</summary>
        /// <param name="dosyaAdi">Dosya adı. (Masaüstüne kaydeder.)</param>
        public static void CSVKaydet(this List<VoronoiBilgisi> list, string dosyaAdi)
        {
            string dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + ".csv";

            //Dosya zaten varsa, numaralandırıp yeni isim oluşturuyoruz.
            for (int i = 1; ; i++)
                if (File.Exists(dosyaYolu))
                    dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + "_" + i.ToString() + ".csv";
                else break;

            StreamWriter dosya = new StreamWriter(dosyaYolu, false, Encoding.UTF8);
            string baslik = "Voronoi ID;Bina Sayısı;Kare Bina;Dikdörgen Bina;Konut;Resmi;Min Alan;Max Alan;Alan;Toplam Bina Alanı;Yogunluk;GENELLEŞTİRME";


            dosya.WriteLine(baslik);
            for (int i = 0; i < list.Count; i++)
                dosya.WriteLine(list[i].ToString());


            dosya.Flush();
            dosya.Close();
        }
        #endregion
        //-----------------------------------------------------------------------------------


        /// <summary>Voronoi ve içindeki binalarla ilgili istatistiki bilgileri analiz eder.</summary>        
        public static VoronoiBilgisi VoronoiAnalizEt(IFeature voronoiFeature)
        {
            ILayer binaLayer = BinaGEN.Layer("Binalar_Yeni2");
            IFeatureClass binaFC = (binaLayer as IFeatureLayer).FeatureClass;
            IFeatureCursor binaCursor;
            IFeature binaFeature;
            List<Bina> voronoiIcindekiBinalar;
            VoronoiBilgisi voronoiBilgi = new VoronoiBilgisi(voronoiFeature);

            try
            {
                voronoiIcindekiBinalar = new List<Bina>();

                binaCursor = BinaGEN.IctekiOgeleriGetir(voronoiFeature, binaFC);
                binaFeature = binaCursor.NextFeature();
                while (binaFeature != null)
                {
                    voronoiIcindekiBinalar.Add(new Bina(binaFeature));
                    binaFeature = binaCursor.NextFeature();
                }

                foreach (Bina bina in voronoiIcindekiBinalar)
                {
                    if (bina.KareMi()) voronoiBilgi.KareBinaSayisi++;
                    if (bina.DikdortgenMi()) voronoiBilgi.DikdortgenBinaSayisi++;
                    if (bina.Type == "Residential Building") voronoiBilgi.KonutSayisi++;
                    if (bina.Type == "Official Building") voronoiBilgi.ResmiBinaSayisi++;
                }

                voronoiBilgi.BinaSayisi = voronoiIcindekiBinalar.Count;
                voronoiBilgi.MaxBinaAlani = Convert.ToInt32(voronoiIcindekiBinalar.Select(b => b.Area).Max());
                voronoiBilgi.MinBinaAlani = Convert.ToInt32(voronoiIcindekiBinalar.Select(b => b.Area).Min());
                voronoiBilgi.BinaToplamAlan = Convert.ToInt32(voronoiIcindekiBinalar.Select(b => b.Area).Sum());
                voronoiBilgi.Alan = Convert.ToInt32((voronoiBilgi.Shape as IArea).Area);
                double yogunluk = (100 * voronoiBilgi.BinaToplamAlan / voronoiBilgi.Alan);
                voronoiBilgi.Yogunluk = Math.Round(yogunluk, 2);
                if (voronoiBilgi.BinaSayisi > 1)
                    voronoiBilgi.UygulanacakGenellestirme = BinaGEN.Vor_GenellestirmeTurunuBelirle(voronoiBilgi);
                else
                    voronoiBilgi.UygulanacakGenellestirme = GenellestirmeTuru.NONE;
                // VORONOİ İŞLEMLERİ BİTTİ--------------------------------------------
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj("HATA", hata.Message);
            }
            return voronoiBilgi;
        }

        /// <summary>Voronoi bilgilerine bağlı olarak Voronoi içinde uygulanacak genelleştirme 
        /// türünü tespit eder.</summary>
        private static GenellestirmeTuru Vor_GenellestirmeTurunuBelirle(VoronoiBilgisi voronoi)
        {
            double yogunluk = voronoi.Yogunluk;
            double max = voronoi.MaxBinaAlani;
            double min = voronoi.MinBinaAlani;
            int binaSayisi = voronoi.BinaSayisi;
            int kareBinaSayisi = voronoi.KareBinaSayisi;
            int dikdortgenBinaSayisi = voronoi.DikdortgenBinaSayisi;
            int konutsayisi = voronoi.KonutSayisi;
            int resmiBinaSayisi = voronoi.ResmiBinaSayisi;

            // Şartları kontrol ediyoruz.
            bool sart1 = yogunluk < 40;
            if (sart1) return GenellestirmeTuru.OTELEME;

            bool sart2 = yogunluk >= 40 && yogunluk <= 85;
            bool sart3 = max == min || (max - min) < 625;
            bool sart4 = binaSayisi == kareBinaSayisi || binaSayisi == dikdortgenBinaSayisi;
            bool sart5 = konutsayisi == binaSayisi || resmiBinaSayisi == binaSayisi;
            bool sart6 = sart2 && sart3 && sart4 && sart5;
            if (sart6) return GenellestirmeTuru.TIPIKLESTIRME;

            bool sart7 = !sart6 && sart2;
            if (sart7) return GenellestirmeTuru.ELEME;

            return GenellestirmeTuru.NONE;
        }

        #region Voronoi Analiz ve Tipikleştirme Analiz (Voronoi2ye bağlı tipikleştirme butonu için)
        public static void VoronoiAnalizEtVeTipiklestir(ILayer voronoiLayer, ILayer tamponLayer, ILayer binaLayer)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap; map.ClearSelection();
            IFeatureClass voronoiFC = (voronoiLayer as IFeatureLayer).FeatureClass,
                          kumeFC = (tamponLayer as IFeatureLayer).FeatureClass,
                          binaFC = (binaLayer as IFeatureLayer).FeatureClass,
                          binaTipikFClass;
            IFeatureCursor voronoiCursor, kumeCursor, binaCursor;
            IFeature voronoi, kume, bina;

            //Binalar üzerinde analiz yapmak için kullanacağımız bina listesi değişkeni;
            List<Bina> binalar, tipiklesmisBinalar, kalanBinalar = new List<Bina>();
            List<BinaMesafe> binaMesafeler;

            // Tipikleştirilmiş binaları tutacak yeni bir feature class oluşturuyoruz.
            string dosyaGDB = BinaGEN.TabakaKlasoruAl(binaLayer);
            string yeniTabakaAdi = "Bina_Tipiklesmis";
            IWorkspaceFactory wsf = new FileGDBWorkspaceFactoryClass();
            IWorkspace2 ws = (IWorkspace2)wsf.OpenFromFile(dosyaGDB, ArcMap.Application.hWnd);
            binaTipikFClass = BinaGEN.YeniFeatureClassOlustur(ws, yeniTabakaAdi, map.SpatialReference, esriGeometryType.esriGeometryPolygon, null);
            // Tipikleştirilmiş binaları tutacak yeni bir tabaka oluşturuyoruz.
            IFeatureLayer binaTipikFLayer = new FeatureLayerClass();
            binaTipikFLayer.FeatureClass = binaTipikFClass;
            binaTipikFLayer.Name = (binaTipikFClass as IDataset).Name;

            //for (int i = 1; i < 100; i++) //Aynı isimde tabaka varsa yeni tabakanın ismini değiştiriyoruz.
            //    if (GEN.TabakaVarMi(map, yeniTabakaAdi))
            //        binaTipikFLayer.Name = yeniTabakaAdi + i.ToString();
            //    else break;

            try
            {
                int binaSayisi;
                ISpatialFilter spatialFilter = new SpatialFilterClass();

                //Voronoilerde teker teker dolaş-------------------------------------
                voronoiCursor = voronoiFC.Search(null, true);
                voronoi = voronoiCursor.NextFeature();
                while (voronoi != null)
                {
                    //Voronoi içine düşen bina sayısı
                    binaSayisi = BinaGEN.IctekiOgeSayisi(voronoi, binaFC);
                    if (binaSayisi == 0)
                    {
                        if (binaSayisi == 1)
                        { //Bina sayısı 1 ise o binayı kalanBinalar tablosuna ekle ve diğer Voronoi'ye geç.
                            binaCursor = BinaGEN.IctekiOgeleriGetir(voronoi, binaFC);
                            bina = binaCursor.NextFeature();
                            kalanBinalar.Add(new Bina(bina));
                        }
                        voronoi = voronoiCursor.NextFeature();
                        continue;
                    }

                    //O anki Voronoi içindeki kümelerde dolaş------------------------
                    kumeCursor = BinaGEN.IctekiOgeleriGetir(voronoi, kumeFC);
                    kume = kumeCursor.NextFeature();
                    while (kume != null)
                    {
                        //Küme içine düşen bina sayısı
                        binaSayisi = BinaGEN.IctekiOgeSayisi(kume, binaFC);
                        if (binaSayisi <= 1)
                        {
                            if (binaSayisi == 1)
                            { //Bina sayısı 1 ise o binayı kalanBinalar tablosuna ekle ve diğer Küme'ye geç.
                                binaCursor = BinaGEN.IctekiOgeleriGetir(kume, binaFC);
                                bina = binaCursor.NextFeature();
                                kalanBinalar.Add(new Bina(bina));
                            }
                            kume = kumeCursor.NextFeature();
                            continue;
                        }

                        binalar = new List<Bina>();
                        binaMesafeler = new List<BinaMesafe>();

                        binaCursor = BinaGEN.IctekiOgeleriGetir(kume, binaFC);
                        bina = binaCursor.NextFeature();
                        //Küme içindeki binaları List<Bina>'ya ekliyoruz.
                        while (bina != null)
                        {
                            binalar.Add(new Bina(bina)); //Binayı binalar koleksiyonuna ekle.           
                            bina = binaCursor.NextFeature();//Sonraki bina                            
                        }

                        //Eğer kümede 2 bina varsa ve ikisi de konveksse diğer kümeye geç.
                        if (binalar.Count == 2 && binalar[0].KonveksMi() && binalar[1].KonveksMi())
                        {
                            kalanBinalar.AddRange(binalar);
                            kume = kumeCursor.NextFeature();//Sonraki küme
                            continue;
                        }

                        //Küme içindeki binalar için BinaMesafe tablosunu oluşturuyoruz.
                        foreach (Bina bina1 in binalar)
                            foreach (Bina bina2 in binalar)
                                if (bina1.BinaID < bina2.BinaID)
                                    binaMesafeler.Add(new BinaMesafe(bina1, bina2));
                        // Küme içindeki Binalar ve bu binalara ait BinaMesafeler tablosu oluşturuldu.
                        // Bundan sonra, tablodaki değerler kullanılarak tipikleştirme yapılacak.


                        //***** ZURNANIN ZIRT DEDİĞİ YER ******//
                        //*************************************//
                        tipiklesmisBinalar = BinaGEN.Tipikleştir(binalar, binaMesafeler);
                        kalanBinalar.AddRange(tipiklesmisBinalar);
                        //*************************************//
                        //*************************************//


                        kume = kumeCursor.NextFeature();//Sonraki küme
                    }
                    // VORONOi İÇİNDEKİ KÜME İŞLEMİ BİTTİ ----------------------------

                    voronoi = voronoiCursor.NextFeature(); //Sonraki voronoi bölgesi
                }
                // VORONOİ İŞLEMLERİ BİTTİ-------------------------------------------- 

                //Kalan binaları yeni tabakaya ekliyorum.
                foreach (Bina b in kalanBinalar)
                {
                    bina = binaTipikFClass.CreateFeature();
                    bina.Shape = b.Shape as IGeometry;
                    bina.Store();
                }

                kalanBinalar.CSVKaydet("Kalan Binalar");
                mxDoc.AddLayer(binaTipikFLayer); //Oluşan featureclass'ı tabaka olarak ekle.                
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj("HATA", hata.Message);
                kalanBinalar.CSVKaydet("Hata");
            }
        }
        #endregion

        #region Genelleştime Bölgesi içinde Tipikleştirme İşlemi
        public static void GBTipiklestir(ILayer gbLayer, ILayer tampon5mLayer, ILayer binaLayer, ILayer gb_5mLayer)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap; map.ClearSelection();
            IFeatureClass gbFC = (gbLayer as IFeatureLayer).FeatureClass,
                          gb_5mFC = (gb_5mLayer as IFeatureLayer).FeatureClass,
                          tampon5mFC = (tampon5mLayer as IFeatureLayer).FeatureClass,
                          binaFC = (binaLayer as IFeatureLayer).FeatureClass,
                          binaTipikFClass;
            IFeatureCursor gbCursor, tampon5mCursor, binaCursor;
            IFeature gbF, tampon5mF, binaF;
            IFields fields = gbFC.Fields as IFields;

            //Binalar üzerinde analiz yapmak için kullanacağımız bina listesi değişkeni;
            List<Bina> binalar, binalarClone, tipiklesmisBinalar, kalanBinalar = new List<Bina>();
            List<BinaMesafe> binaMesafeler;
            List<Tampon> tamponList;
            Bina bina;

            // Tipikleştirilmiş binaları tutacak yeni bir feature class oluşturuyoruz.
            string dosyaGDB = BinaGEN.TabakaKlasoruAl(binaLayer);
            string yeniTabakaAdi = "Bina_GB_Tipiklesmis";
            IWorkspaceFactory wsf = new FileGDBWorkspaceFactoryClass();
            IWorkspace2 ws = (IWorkspace2)wsf.OpenFromFile(dosyaGDB, ArcMap.Application.hWnd);

            // bina fieldlarını yeni tipikleşmiş bina featureclassına kopyalıyoruz
            IFields oldFields = binaFC.Fields;
            IClone cloneSource = (IClone)oldFields;
            IClone cloneTarget = cloneSource.Clone();
            IFields yeniBinaFields = (IFields)cloneTarget;

            binaTipikFClass = BinaGEN.YeniFeatureClassOlustur(ws, yeniTabakaAdi, map.SpatialReference,
                esriGeometryType.esriGeometryPolygon, yeniBinaFields);
            // Tipikleştirilmiş binaları tutacak yeni bir tabaka oluşturuyoruz.
            IFeatureLayer binaTipikFLayer = new FeatureLayerClass();
            binaTipikFLayer.FeatureClass = binaTipikFClass;
            binaTipikFLayer.Name = (binaTipikFClass as IDataset).Name;

            try
            {
                ISpatialFilter spatialFilter = new SpatialFilterClass();
                GenellestirmeTuru genTurEnum;
                int genTurFieldNo = fields.FindField("GENELLESTIRME");
                string genTur;

                string dosyaAdi = "GB_Tipiklestirme_Sonuc";
                string dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + ".csv";

                //Dosya zaten varsa, numaralandırıp yeni isim oluşturuyoruz.
                for (int i = 1; ; i++)
                    if (File.Exists(dosyaYolu))
                        dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + "_" + i.ToString() + ".csv";
                    else break;
                StreamWriter file = new StreamWriter(dosyaYolu, false, Encoding.UTF8);
                file.WriteLine("ID;GenTürü;İlkBinaSayısı;SonBinaSayısı;Açıklama");


                //Genelleştirme Bölgesi içinde teker teker dolaş-------------------------------------
                gbCursor = gbFC.Search(null, true);
                gbF = gbCursor.NextFeature();
                while (gbF != null)
                {
                    int konveksSayisi = 0;

                    binaCursor = BinaGEN.IctekiOgeleriGetir(gbF, binaFC);
                    binaF = binaCursor.NextFeature();
                    if (binaF == null)
                    { //gb içinde bina olmaması ihtimaline binaen
                        gbF = gbCursor.NextFeature();
                        continue;
                    }

                    genTur = gbF.get_Value(genTurFieldNo).ToString();
                    if (genTur != "TIPIKLESTIRME")
                    {
                        //Genelleştirme türü "Tipikleştirme" olmayan binaları kalan binalar listesine ekliyoruz.                        
                        while (binaF != null)
                        {
                            kalanBinalar.Add(new Bina(binaF));
                            binaF = binaCursor.NextFeature();
                        }
                        gbF = gbCursor.NextFeature();
                        continue;
                    }

                    //Genelleştirme türü "Tipikleştirme" olan bina gruplarını analiz etmek ve tipikleştirme
                    //algoritmasını uygulamak için binalar listesi oluşturuyoruz.
                    binalar = new List<Bina>();
                    binalarClone = new List<Bina>();
                    while (binaF != null)
                    {
                        binalar.Add(bina = new Bina(binaF));
                        binalarClone.Add(new Bina(binaF));
                        if (bina.KonveksMi() == true) konveksSayisi++;
                        binaF = binaCursor.NextFeature();
                    }
                    if (binalar.Count == konveksSayisi)
                    { //eğer binaların hepsi konveksse kalan binalara ekle ve diğer gb'ye geç.
                        kalanBinalar.AddRange(binalar);
                        gbF = gbCursor.NextFeature();
                        continue;
                    }

                    //Eğer genelleştirme türü Tipikleştirme ise ve tüm binalar konveks değilse buradan devam ediyoruz.//
                    //GB içindeki binalar için BinaMesafe tablosunu oluşturuyoruz.
                    binaMesafeler = new List<BinaMesafe>();
                    foreach (Bina bina1 in binalar)
                        foreach (Bina bina2 in binalar)
                            if (bina1.BinaID < bina2.BinaID)
                                binaMesafeler.Add(new BinaMesafe(bina1, bina2));

                    tamponList = new List<Tampon>();
                    tampon5mCursor = BinaGEN.IctekiOgeleriGetir(gbF, tampon5mFC);
                    tampon5mF = tampon5mCursor.NextFeature();
                    while (tampon5mF != null)
                    {
                        tamponList.Add(new Tampon(tampon5mF));
                        tampon5mF = tampon5mCursor.NextFeature();
                    }

                    //GB ile aynı OID'ye sahip GB_5m'nin alanını alıyoruz.
                    double gb_5mAlan = (gb_5mFC.GetFeature(gbF.OID).Shape as IArea).Area;

                    GenellestirmeBolgesi genBolgesi = new GenellestirmeBolgesi(gbF);
                    double genBolStandartMesafe = BinaGEN.GB_GenBolStandartMesafe(genBolgesi);


                    //Binaları Tipikleştirmeye gönder
                    tipiklesmisBinalar = BinaGEN.GB_Tipikleştir(binalar, binaMesafeler, tamponList, genBolgesi, genBolStandartMesafe, gb_5mAlan, binalar.Count); /***********/
                    if (tipiklesmisBinalar == null)
                    {//Tipikleştirme sonucu null dönerse, bu GB'de tipikleştirme yerine kaynaştırma kullanılacak demektir.
                        //kaynaştırma uygulanacak binaları kalan binalara ekle, diğer GB'ye geç
                        file.WriteLine(gbF.OID.ToString() + ";KAYNASTIRMA;;;");
                        kalanBinalar.AddRange(binalarClone);
                        genTurEnum = GenellestirmeTuru.KAYNASTIRMA;
                        gbF.set_Value(genTurFieldNo, Enum.GetName(typeof(GenellestirmeTuru), (int)genTurEnum));
                        gbF.Store();
                        gbF = gbCursor.NextFeature();
                        continue;
                    }
                    file.WriteLine(gbF.OID.ToString() + "TIPIKLESTIRME" + binalar.Count.ToString() + ";" + tipiklesmisBinalar.Count.ToString() + ";");
                    kalanBinalar.AddRange(tipiklesmisBinalar);
                    gbF.set_Value(genTurFieldNo, "TIPIKLESTIRILDI");
                    gbF.Store();
                    gbF = gbCursor.NextFeature(); //Sonraki genelleştirme bölgesi
                }

                //Kalan binaları yeni tabakaya ekliyorum.
                IFeatureCursor binaTipikFClassCursor = binaTipikFClass.Insert(true);
                foreach (Bina b in kalanBinalar)
                {
                    b.Feature.Shape = b.Shape as IGeometry;
                    binaTipikFClassCursor.InsertFeature(b.Feature as IFeatureBuffer);
                    binaTipikFClassCursor.Flush();
                }

                file.Flush();
                file.Close();
                kalanBinalar.CSVKaydet("Kalan Binalar");
                mxDoc.AddLayer(binaTipikFLayer); //Oluşan featureclass'ı tabaka olarak ekle.                
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj("HATA", hata.Message);
                kalanBinalar.CSVKaydet("Hata");
            }
        }
        #endregion

        #region Genelleştirme Bölgesi İçinde Uygulanacak Tipikleştirme İşlemi *REKÜRSİF Fonksiyon*
        //DİNAMİK YOĞUNLUK KONTROLÜ YAPILARAK BİMA SAYISI ÇOK AZALIRSA BİNALARI KAYNAŞTIRACAĞIZ
        /// <summary> List&lt;BinaMesafe&gt; tablosundaki bina mesafeleri değerine
        /// göre List&lt;Bina&gt; içindeki binaları tipikleştirir.</summary>
        /// <param name="binalar">Binakları içeren liste</param>
        /// <param name="binaMesafeTablo">Binalar arasındaki mesafeleri içeren liste</param>
        private static List<Bina> GB_Tipikleştir(List<Bina> binalar, List<BinaMesafe> binaMesafeTablo, List<Tampon> tampon, GenellestirmeBolgesi genBolgesi, double gbStdMesafe, double gb_5mAlan, int binaSayisi)
        {
            List<BinaMesafe> binaMesafeTablo2 = new List<BinaMesafe>();
            Bina yeniBina, bina1, bina2;
            double binaAgirlikliMesafe, oran, binalarOrtalamaAlan;
            double yogunlukLimit, genBolYogunluk, tamponToplamAlan;
            bool binaSayisiSarti;

            try
            {
                // binaMesafeTablosunda iki binası da konveks olan satırları sil.
                foreach (BinaMesafe kayit in binaMesafeTablo)
                {
                    bina1 = binalar.GetByID(kayit.Bina1_ID);
                    bina2 = binalar.GetByID(kayit.Bina2_ID);
                    if (!(bina1.KonveksMi() || bina2.KonveksMi()))
                    {
                        binaMesafeTablo2.Add(kayit);
                    }
                }
                if (binaMesafeTablo2.Count == 0) return binalar;

                // Tabloyu mesafelere göre küçükten büyüğe sıralıyoruz.
                binaMesafeTablo2 = binaMesafeTablo2.OrderBy(k => k.Mesafe).ToList<BinaMesafe>();
                //binaMesafeTablo2 = binaMesafeTablo2.Where(k => k.Mesafe < 10).OrderBy(k => k.Mesafe).ToList<BinaMesafe>();

                if (binaMesafeTablo2[0].Mesafe < 10 /*Minimum mesafe*/)
                {
                    bina1 = binalar.GetByID(binaMesafeTablo2[0].Bina1_ID);
                    bina2 = binalar.GetByID(binaMesafeTablo2[0].Bina2_ID);
                    yeniBina = bina1 | bina2;

                    if (yeniBina.BinaID == bina1.BinaID)
                    {
                        binalar.Remove(bina2);
                        tampon.RemoveAt(tampon.FindIndex(t => t.ID == bina2.BinaID));
                    }
                    else
                    {
                        binalar.Remove(bina1);
                        tampon.RemoveAt(tampon.FindIndex(t => t.ID == bina1.BinaID));
                    }

                    //Tek bina kaldığında binaMesafeTablo2'de 0 kayıt olacaktır.
                    //Tek bina kalmışsa o tek binayı döndürüyoruz.
                    if (binalar.Count == 1) return binalar;

                    tamponToplamAlan = tampon.Select(a => a.Area).Sum();
                    binaAgirlikliMesafe = BinaGEN.GB_BinaAgirlikliMesafe(binalar);
                    oran = gbStdMesafe / binaAgirlikliMesafe;
                    binalarOrtalamaAlan = binalar.Select(b => b.Area).Average();
                    genBolYogunluk = tamponToplamAlan / gb_5mAlan;
                    yogunlukLimit = (80 + 2 * (oran - tamponToplamAlan / binalarOrtalamaAlan)) / 100;

                    binaSayisiSarti = binalar.Count > Math.Floor(binaSayisi * Math.Sqrt(2) / 4);

                    //Herhangi bir şekilde, bina sayısı şartı sağlanmıyorsa null değer gönder
                    //(Kaynaştırma yapılması gerek)                    
                    if (!binaSayisiSarti) return null;

                    //bina sayısı şartı ve yoğunluk şartı sağlanıyorsa kalan binaları gönder
                    if (genBolYogunluk < yogunlukLimit)
                        return binalar;

                    //bina sayısı şartı sağlanıyor ama yoğunluk şartı sağlanmıyorsa tipikleştirmeye devam et.
                    binaMesafeTablo2 = binalar.MesafeTablosuOlustur();
                    binalar = GB_Tipikleştir(binalar, binaMesafeTablo2, tampon, genBolgesi, gbStdMesafe, gb_5mAlan, binaSayisi);
                }
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj("HATA", hata.Message + "\n" + "Bina Numaraları: " + binalar.BinaNumaralariGetir());
            }

            return binalar;
        }
        #endregion

        #region Tipikleştirme Algoritması *REKÜRSİF Fonksiyon*
        /// <summary> List&lt;BinaMesafe&gt; tablosundaki bina mesafeleri değerine
        /// göre List&lt;Bina&gt; içindeki binaları tipikleştirir.</summary>
        /// <param name="binalar">Binakları içeren liste</param>
        /// <param name="binaMesafeTablo">Binalar arasındaki mesafeleri içeren liste</param>
        public static List<Bina> Tipikleştir(List<Bina> binalar, List<BinaMesafe> binaMesafeTablo)
        {
            List<BinaMesafe> binaMesafeTablo2 = new List<BinaMesafe>();
            Bina yeniBina, bina1, bina2;
            try
            {
                // binaMesafeTablosunda iki binası da konveks olan satırları sil.
                foreach (BinaMesafe kayit in binaMesafeTablo)
                {
                    bina1 = binalar.GetByID(kayit.Bina1_ID);
                    bina2 = binalar.GetByID(kayit.Bina2_ID);
                    if (!(bina1.KonveksMi() || bina2.KonveksMi()))
                    {
                        binaMesafeTablo2.Add(kayit);
                    }
                }
                if (binaMesafeTablo2.Count == 0) return binalar;

                // Tabloyu mesafelere göre küçükten büyüğe sıralıyoruz.
                binaMesafeTablo2 = binaMesafeTablo2.OrderBy(k => k.Mesafe).ToList<BinaMesafe>();
                //binaMesafeTablo2 = binaMesafeTablo2.Where(k => k.Mesafe < 10).OrderBy(k => k.Mesafe).ToList<BinaMesafe>();

                if (binaMesafeTablo2[0].Mesafe < 10 /*Minimum mesafe*/)
                {
                    bina1 = binalar.GetByID(binaMesafeTablo2[0].Bina1_ID);
                    bina2 = binalar.GetByID(binaMesafeTablo2[0].Bina2_ID);
                    yeniBina = bina1 | bina2;

                    if (yeniBina.BinaID == bina1.BinaID) binalar.Remove(bina2);
                    else binalar.Remove(bina1);

                    //Tek bina kaldığında binaMesafeTablo2'de 0 kayıt olacaktır.
                    //Tek bina kalmışsa o tek binayı döndürüyoruz.
                    if (binalar.Count == 1) return binalar;

                    binaMesafeTablo2 = binalar.MesafeTablosuOlustur();
                    binalar = Tipikleştir(binalar, binaMesafeTablo2);
                }
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj("HATA", hata.Message + "\n" + "Bina Numaraları: " + binalar.BinaNumaralariGetir());
            }

            return binalar;
        }
        #endregion

        /// <summary>Voronoi feature class tablosuna istatistiki bilgileri tuması için 
        /// alanlar (field) ekler.</summary>        
        public static void Voronoi_AlanEkle(IFeatureClass voronoiFC)
        {
            IFields fields = voronoiFC.Fields as IFields;
            IFieldEdit yeniField = new FieldClass();

            yeniField = new FieldClass();
            string fieldName = "BINA_SAYISI";
            yeniField.AliasName_2 = "Bina Sayısı";
            yeniField.Name_2 = fieldName;
            yeniField.Type_2 = esriFieldType.esriFieldTypeInteger;
            if (fields.FindField(fieldName) < 0) voronoiFC.AddField(yeniField);

            yeniField = new FieldClass();
            fieldName = "KARE_BINA";
            yeniField.AliasName_2 = "Kare Bina";
            yeniField.Name_2 = fieldName;
            yeniField.Type_2 = esriFieldType.esriFieldTypeInteger;
            if (fields.FindField(fieldName) < 0) voronoiFC.AddField(yeniField);

            yeniField = new FieldClass();
            fieldName = "DIKDORTGEN_BINA";
            yeniField.AliasName_2 = "Dikdörtgen Bina";
            yeniField.Name_2 = fieldName;
            yeniField.Type_2 = esriFieldType.esriFieldTypeInteger;
            if (fields.FindField(fieldName) < 0) voronoiFC.AddField(yeniField);

            yeniField = new FieldClass();
            fieldName = "KONUT";
            yeniField.AliasName_2 = "Konut";
            yeniField.Name_2 = fieldName;
            yeniField.Type_2 = esriFieldType.esriFieldTypeInteger;
            if (fields.FindField(fieldName) < 0) voronoiFC.AddField(yeniField);

            yeniField = new FieldClass();
            fieldName = "RESMI";
            yeniField.AliasName_2 = "Resmi Bina";
            yeniField.Name_2 = fieldName;
            yeniField.Type_2 = esriFieldType.esriFieldTypeInteger;
            if (fields.FindField(fieldName) < 0) voronoiFC.AddField(yeniField);

            yeniField = new FieldClass();
            fieldName = "MIN_ALAN";
            yeniField.AliasName_2 = "Min Alan";
            yeniField.Name_2 = fieldName;
            yeniField.Type_2 = esriFieldType.esriFieldTypeDouble;
            if (fields.FindField(fieldName) < 0) voronoiFC.AddField(yeniField);

            yeniField = new FieldClass();
            fieldName = "MAX_ALAN";
            yeniField.AliasName_2 = "Max Alan";
            yeniField.Name_2 = fieldName;
            yeniField.Type_2 = esriFieldType.esriFieldTypeDouble;
            if (fields.FindField(fieldName) < 0) voronoiFC.AddField(yeniField);

            yeniField = new FieldClass();
            fieldName = "TOPLAM_BINA_ALANI";
            yeniField.Name_2 = fieldName;
            yeniField.AliasName_2 = "Toplam Bina Alanı";
            yeniField.Type_2 = esriFieldType.esriFieldTypeDouble;
            if (fields.FindField(fieldName) < 0) voronoiFC.AddField(yeniField);

            yeniField = new FieldClass();
            fieldName = "YOGUNLUK";
            yeniField.AliasName_2 = "Yoğunluk";
            yeniField.Name_2 = fieldName;
            yeniField.Type_2 = esriFieldType.esriFieldTypeDouble;
            if (fields.FindField(fieldName) < 0) voronoiFC.AddField(yeniField);

            yeniField = new FieldClass();
            fieldName = "GENELLESTIRME";
            yeniField.Name_2 = fieldName;
            yeniField.AliasName_2 = "Genelleştirme";
            yeniField.Type_2 = esriFieldType.esriFieldTypeString;
            if (fields.FindField(fieldName) < 0) voronoiFC.AddField(yeniField);

        }

        /// <summary> Voronoi ile ilgili istatistiki bilgileri Voronoi tablosuna kaydeder.</summary>
        public static void VoronoiAnalizKaydet(IFeature voronoi, VoronoiBilgisi voronoiBilgi)
        {
            IFields fields = voronoi.Fields as IFields;
            GenellestirmeTuru genellestirmeTuru;
            string fieldName;

            fieldName = "BINA_SAYISI"; voronoi.set_Value(fields.FindField(fieldName), voronoiBilgi.BinaSayisi);
            fieldName = "KARE_BINA"; voronoi.set_Value(fields.FindField(fieldName), voronoiBilgi.KareBinaSayisi);
            fieldName = "DIKDORTGEN_BINA"; voronoi.set_Value(fields.FindField(fieldName), voronoiBilgi.DikdortgenBinaSayisi);
            fieldName = "KONUT"; voronoi.set_Value(fields.FindField(fieldName), voronoiBilgi.KonutSayisi);
            fieldName = "RESMI"; voronoi.set_Value(fields.FindField(fieldName), voronoiBilgi.ResmiBinaSayisi);
            fieldName = "MIN_ALAN"; voronoi.set_Value(fields.FindField(fieldName), voronoiBilgi.MinBinaAlani);
            fieldName = "MAX_ALAN"; voronoi.set_Value(fields.FindField(fieldName), voronoiBilgi.MaxBinaAlani);
            fieldName = "TOPLAM_BINA_ALANI"; voronoi.set_Value(fields.FindField(fieldName), voronoiBilgi.BinaToplamAlan);
            fieldName = "YOGUNLUK"; voronoi.set_Value(fields.FindField(fieldName), voronoiBilgi.Yogunluk);
            fieldName = "GENELLESTIRME"; genellestirmeTuru = voronoiBilgi.UygulanacakGenellestirme;
            voronoi.set_Value(fields.FindField(fieldName), Enum.GetName(typeof(GenellestirmeTuru), (int)genellestirmeTuru));

            voronoi.Store();
        }

        public static void GenBolAnalizKaydet(IFeature genBol, VoronoiBilgisi voronoiBilgi)
        {/*BU FONKSİYONU DAHA DÜZELTMEDİM*/
            IFields fields = genBol.Fields as IFields;
            GenellestirmeTuru genellestirmeTuru;
            string fieldName;

            fieldName = "BINA_SAYISI"; genBol.set_Value(fields.FindField(fieldName), voronoiBilgi.BinaSayisi);
            fieldName = "KARE_BINA"; genBol.set_Value(fields.FindField(fieldName), voronoiBilgi.KareBinaSayisi);
            fieldName = "DIKDORTGEN_BINA"; genBol.set_Value(fields.FindField(fieldName), voronoiBilgi.DikdortgenBinaSayisi);
            fieldName = "KONUT"; genBol.set_Value(fields.FindField(fieldName), voronoiBilgi.KonutSayisi);
            fieldName = "RESMI"; genBol.set_Value(fields.FindField(fieldName), voronoiBilgi.ResmiBinaSayisi);
            fieldName = "MIN_ALAN"; genBol.set_Value(fields.FindField(fieldName), voronoiBilgi.MinBinaAlani);
            fieldName = "MAX_ALAN"; genBol.set_Value(fields.FindField(fieldName), voronoiBilgi.MaxBinaAlani);
            fieldName = "TOPLAM_BINA_ALANI"; genBol.set_Value(fields.FindField(fieldName), voronoiBilgi.BinaToplamAlan);
            fieldName = "YOGUNLUK"; genBol.set_Value(fields.FindField(fieldName), voronoiBilgi.Yogunluk);
            fieldName = "GENELLESTIRME"; genellestirmeTuru = voronoiBilgi.UygulanacakGenellestirme;
            genBol.set_Value(fields.FindField(fieldName), Enum.GetName(typeof(GenellestirmeTuru), (int)genellestirmeTuru));

            genBol.Store();
        }

        /* GENELLEŞTİRME TÜRÜ BELİRLEME */

        public static void GB_GenellestirmeTuruBelirle(ILayer gbLayer, ILayer gbLayer_5m, ILayer binaLayer, ILayer tamponLayer)
        {
            IFeatureClass genBolFC_5m = (gbLayer_5m as IFeatureLayer).FeatureClass,
                          genBolFC = (gbLayer as IFeatureLayer).FeatureClass,
                          binaFC = (binaLayer as IFeatureLayer).FeatureClass,
                           tamponFC = (tamponLayer as IFeatureLayer).FeatureClass;
            IFeatureCursor genBolCursor, tamponCursor, binaCursor;
            IFeature genBolFeature_5m, genBolFeature, tamponFeature, bina;
            ITopologicalOperator binaTopOp;
            List<Bina> binalar;
            GenellestirmeBolgesi genBolgesi;
            GenellestirmeTuru genellestirmeTuru;
            IPolygon kalanGeometri;
            double kalanGeometriAlani, tamponToplamAlan;

            int binaSayisi;
            ISpatialFilter spatialFilter = new SpatialFilterClass();

            //Genelleştirme türünü kaydedeceğimiz sütun (field) oluşturuyoruz.
            IFields fields = genBolFC.Fields as IFields;
            string fieldName = "GENELLESTIRME";
            if (fields.FindField(fieldName) < 0)
            {
                IFieldEdit yeniField = new FieldClass();
                yeniField = new FieldClass();
                yeniField.Name_2 = fieldName;
                yeniField.AliasName_2 = "Genelleştirme";
                yeniField.Type_2 = esriFieldType.esriFieldTypeString;
                genBolFC.AddField(yeniField);
            }

            string dosyaAdi = "GBAnaliz";
            string dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + ".csv";

            //Dosya zaten varsa, numaralandırıp yeni isim oluşturuyoruz.
            for (int i = 1; ; i++)
                if (File.Exists(dosyaYolu))
                    dosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + dosyaAdi + "_" + i.ToString() + ".csv";
                else break;
            StreamWriter file = new StreamWriter(dosyaYolu, false, Encoding.UTF8);
            file.WriteLine("ID;BinaSayısı;GBStandartMesafe;BinaAğırlıklıMesafe;Oran;BinalarOrtalamaAlan;YoğunlukLimit;GBYoğunluğu");
            //Genelleştirme bölgeleri içinde dolaşmaya başlıyoruz.
            genBolCursor = genBolFC.Search(null, true);
            genBolFeature = genBolCursor.NextFeature();
            while (genBolFeature != null)
            {
                genBolgesi = new GenellestirmeBolgesi(genBolFeature);
                try { genBolFeature_5m = genBolFC_5m.GetFeature(genBolFeature.OID); }
                catch (Exception hata) { BinaGEN.Mesaj(hata.Source, hata.Message); return; }

                binalar = new List<Bina>();
                tamponToplamAlan = 0;

                binaCursor = BinaGEN.IctekiOgeleriGetir(genBolFeature, binaFC);
                bina = binaCursor.NextFeature();
                tamponCursor = BinaGEN.IctekiOgeleriGetir(genBolFeature, tamponFC);
                tamponFeature = tamponCursor.NextFeature();

                while (bina != null)
                {
                    binalar.Add(new Bina(bina));//Genelleştirme bölgesi içindeki binaları List<Bina>'ya ekliyoruz.
                    bina = binaCursor.NextFeature();
                    tamponToplamAlan += (tamponFeature.Shape as IArea).Area;
                    tamponFeature = tamponCursor.NextFeature();
                }

                int genFieldNo = fields.FindField("GENELLESTIRME");
                double genBolStandartMesafe, binaAgirlikliMesafe, oran, binalarOrtalamaAlan, yogunlukLimit, genBolYogunluk, genBolgesiAlan_5m;

                binaSayisi = binalar.Count;
                if (binaSayisi == 1)
                {
                    binaTopOp = binalar[0].Shape as ITopologicalOperator;
                    //Bina geometrisinden GB geometrisini çıkarıyoruz. Eğer kalan
                    //geometri alanı 0 ise bina tamamen GB içindedir.
                    kalanGeometri = binaTopOp.Difference(genBolFeature.Shape) as IPolygon;


                    kalanGeometriAlani = (kalanGeometri as IArea).Area;
                    //Eğer bina tamamen GB içinde ise genelleştirme türü "-"
                    //yapılıp diğer GB'ye geçiyoruz.
                    if (kalanGeometriAlani == 0)
                    {
                        genBolFeature.set_Value(genFieldNo, "-");
                        genBolFeature.Store();
                        genBolFeature = genBolCursor.NextFeature();
                        continue;
                    }
                }



                genBolgesiAlan_5m = (genBolFeature_5m.Shape as IArea).Area;
                genBolStandartMesafe = BinaGEN.GB_GenBolStandartMesafe(genBolgesi);
                binaAgirlikliMesafe = BinaGEN.GB_BinaAgirlikliMesafe(binalar);
                oran = genBolStandartMesafe / binaAgirlikliMesafe;
                if (binaSayisi == 1) oran = 1;
                binalarOrtalamaAlan = binalar.Select(b => b.Area).Average();
                genBolYogunluk = tamponToplamAlan / genBolgesiAlan_5m;
                yogunlukLimit = (80 + 2 * (oran - tamponToplamAlan / binalarOrtalamaAlan)) / 100;

                if (genBolYogunluk < yogunlukLimit)
                {
                    genellestirmeTuru = GenellestirmeTuru.OTELEME;
                    genBolFeature.set_Value(genFieldNo, Enum.GetName(typeof(GenellestirmeTuru), (int)genellestirmeTuru));
                }
                else if (binaSayisi == 1)
                {
                    genellestirmeTuru = GenellestirmeTuru.ELEME;
                    genBolFeature.set_Value(genFieldNo, Enum.GetName(typeof(GenellestirmeTuru), (int)genellestirmeTuru));
                }
                else
                {
                    genellestirmeTuru = GenellestirmeTuru.TIPIKLESTIRME;
                    genBolFeature.set_Value(genFieldNo, Enum.GetName(typeof(GenellestirmeTuru), (int)genellestirmeTuru));
                }

                file.Write(genBolFeature.OID + ";");
                file.Write(binaSayisi + ";");
                file.Write(genBolStandartMesafe.ToString(CultureInfo.InvariantCulture) + ";");
                file.Write(binaAgirlikliMesafe.ToString(CultureInfo.InvariantCulture) + ";");
                file.Write(oran.ToString(CultureInfo.InvariantCulture) + ";");
                file.Write(binalarOrtalamaAlan.ToString(CultureInfo.InvariantCulture) + ";");
                file.Write(yogunlukLimit.ToString(CultureInfo.InvariantCulture) + ";");
                file.WriteLine(genBolYogunluk.ToString(CultureInfo.InvariantCulture));

                genBolFeature.Store();
                genBolFeature = genBolCursor.NextFeature();

            }
            file.Close();

        }

        public static double GB_GenBolStandartMesafe(GenellestirmeBolgesi genBol)
        {
            IPointCollection koseNoktalar = genBol.Shape as IPointCollection;
            int koseSayisi = koseNoktalar.PointCount;
            double centroidX = genBol.Centroid.X, centroidY = genBol.Centroid.Y;
            double araDeger = 0.0, koseX, koseY, standartMesafe;


            for (int i = 0; i < koseSayisi; i++)
            {
                koseX = koseNoktalar.get_Point(i).X;
                koseY = koseNoktalar.get_Point(i).Y;
                araDeger += Math.Sqrt(Math.Pow((koseX - centroidX), 2) + Math.Pow((koseY - centroidY), 2));
            }
            standartMesafe = araDeger / Math.Sqrt(koseSayisi);
            return standartMesafe;
        }

        public static double GB_BinaAgirlikliMesafe(List<Bina> binalar)
        {
            int binaSayisi = binalar.Count;
            double binaToplamAlan = binalar.Select(b => b.Area).Sum();
            double araDeger = 0.0, agirlikliMesafe = 0.0;
            double centroidX, centroidY, alan, binalarAgirlikM_X = 0.0, binalarAgirlikM_Y = 0.0;
            double binalarAgirlikM_ToplamX = 0.0, binalarAgirlikM_ToplamY = 0.0;

            for (int i = 0; i < binaSayisi; i++)
            {
                centroidX = binalar[i].Centroid.X;
                centroidY = binalar[i].Centroid.Y;
                alan = binalar[i].Area;
                binalarAgirlikM_ToplamX += centroidX * alan;
                binalarAgirlikM_ToplamY += centroidY * alan;
            }

            binalarAgirlikM_X = binalarAgirlikM_ToplamX / binaToplamAlan;
            binalarAgirlikM_Y = binalarAgirlikM_ToplamY / binaToplamAlan;

            for (int i = 0; i < binaSayisi; i++)
            {
                centroidX = binalar[i].Centroid.X;
                centroidY = binalar[i].Centroid.Y;
                //sonuc = Toplam [ [(x - a)^2 + (y - b)^2]^(1/2) ]
                araDeger += Math.Sqrt(Math.Pow(centroidX - binalarAgirlikM_X, 2) + Math.Pow(centroidY - binalarAgirlikM_Y, 2));
            }

            agirlikliMesafe = araDeger / Math.Sqrt(binaSayisi);

            return agirlikliMesafe;
        }

        /// <summary>Bir tabakadaki öğelerin geometrisinden oluşan List öğesi döndürür</summary>
        /// <param name="layer">Öğelerine ulaşılacak tabaka ismi</param>
        public static List<IGeometry> TabakadakiOgeGeometrileriniGetir(string tabakaIsmi)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap;
            ILayer tabaka = BinaGEN.Layer(tabakaIsmi);
            IFeatureClass fClass = (tabaka as IFeatureLayer).FeatureClass;
            IFeatureCursor fCursor;
            IFeature feature;
            List<IGeometry> geometryList = new List<IGeometry>();
            ISpatialFilter spatialFilter = new SpatialFilterClass();

            fCursor = fClass.Search(null, true);
            feature = fCursor.NextFeature();
            while (feature != null)
            {
                geometryList.Add(feature.ShapeCopy as IGeometry);
                feature = fCursor.NextFeature();
            }

            return geometryList;
        }

        /// <summary>Bir tabakadaki öğelerin geometrisinden oluşan List öğesi döndürür</summary>
        /// <param name="layer">Öğelerine ulaşılacak tabaka indexi</param>
        public static List<IGeometry> TabakadakiOgeGeometrileriniGetir(int tabakaIndex)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap;
            ILayer tabaka = BinaGEN.Layer(tabakaIndex);
            IFeatureClass fClass = (tabaka as IFeatureLayer).FeatureClass;
            IFeatureCursor fCursor;
            IFeature feature;
            List<IGeometry> geometryList = new List<IGeometry>();
            ISpatialFilter spatialFilter = new SpatialFilterClass();

            fCursor = fClass.Search(null, true);
            feature = fCursor.NextFeature();
            while (feature != null)
            {
                geometryList.Add(feature.ShapeCopy as IGeometry);
                feature = fCursor.NextFeature();
            }

            return geometryList;
        }

        /// <summary>Bir tabakadaki öğelerin geometrisinden oluşan List öğesi döndürür</summary>
        /// <param name="layer">Öğelerine ulaşılacak tabaka</param>
        public static List<IGeometry> TabakadakiOgeGeometrileriniGetir(ILayer tabaka)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap;
            IFeatureClass fClass = (tabaka as IFeatureLayer).FeatureClass;
            IFeatureCursor fCursor;
            IFeature feature;
            List<IGeometry> geometryList = new List<IGeometry>();
            ISpatialFilter spatialFilter = new SpatialFilterClass();

            fCursor = fClass.Search(null, true);
            feature = fCursor.NextFeature();
            while (feature != null)
            {
                geometryList.Add(feature.ShapeCopy as IGeometry);
                feature = fCursor.NextFeature();
            }

            return geometryList;
        }

        /// <summary>Bir tabakadaki öğelerden oluşan List öğesi döndürür</summary>
        /// <param name="layer">Öğelerine ulaşılacak tabaka ismi</param>
        public static List<IFeature> TabakadakiOgeleriGetir(string tabakaIsmi)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap;
            ILayer tabaka = BinaGEN.Layer(tabakaIsmi);
            IFeatureClass fClass = (tabaka as IFeatureLayer).FeatureClass;
            IFeatureCursor fCursor;
            IFeature feature;
            List<IFeature> featureList = new List<IFeature>();
            ISpatialFilter spatialFilter = new SpatialFilterClass();

            fCursor = fClass.Search(null, true);
            feature = fCursor.NextFeature();
            while (feature != null)
            {
                featureList.Add(feature);
                feature = fCursor.NextFeature();
            }

            return featureList;
        }

        /// <summary>Bir tabakadaki öğelerden oluşan List öğesi döndürür</summary>
        /// <param name="layer">Öğelerine ulaşılacak tabaka indexi</param>
        public static List<IFeature> TabakadakiOgeleriGetir(int tabakaIndex)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap;
            ILayer tabaka = BinaGEN.Layer(tabakaIndex);
            IFeatureClass fClass = (tabaka as IFeatureLayer).FeatureClass;
            IFeatureCursor fCursor;
            IFeature feature;
            List<IFeature> featureList = new List<IFeature>();
            ISpatialFilter spatialFilter = new SpatialFilterClass();

            fCursor = fClass.Search(null, true);
            feature = fCursor.NextFeature();
            while (feature != null)
            {
                featureList.Add(feature);
                feature = fCursor.NextFeature();
            }

            return featureList;
        }

        /// <summary>Bir tabakadaki öğelerden oluşan List öğesi döndürür</summary>
        /// <param name="layer">Öğelerine ulaşılacak tabaka</param>
        public static List<IFeature> TabakadakiOgeleriGetir(ILayer tabaka)
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IActiveView activeView = mxDoc.ActiveView;
            IMap map = mxDoc.FocusMap;
            IFeatureClass fClass = (tabaka as IFeatureLayer).FeatureClass;
            IFeatureCursor fCursor;
            IFeature feature;
            List<IFeature> featureList = new List<IFeature>();
            ISpatialFilter spatialFilter = new SpatialFilterClass();

            fCursor = fClass.Search(null, false);
            feature = fCursor.NextFeature();
            while (feature != null)
            {
                featureList.Add(feature);
                feature = fCursor.NextFeature();
            }

            return featureList;
        }

        #region "Create JPEG from ActiveView"
        ///<summary>Creates a .jpg (JPEG) file from IActiveView. Default values of 96 DPI are used for the image creation.</summary>
        ///<param name="activeView">An IActiveView interface</param>
        ///<param name="pathFileName">A System.String that the path and filename of the JPEG you want to create. Example: "C:\temp\test.jpg"</param>
        ///<returns>A System.Boolean indicating the success</returns>
        public static Boolean FeatureJpgOnizlemeOlustur(IActiveView activeView, String pathFileName)
        {
            IMap map = activeView as IMap;
            //parameter check
            if (activeView == null || !(pathFileName.EndsWith(".jpg")))
            {
                return false;
            }

            IExport export = new ExportJPEGClass();
            export.ExportFileName = pathFileName;

            // Microsoft Windows default DPI resolution
            export.Resolution = 96;
            tagRECT rect = activeView.ExportFrame;
            IEnvelope envelope = new EnvelopeClass();
            envelope.PutCoords(rect.left, rect.top, rect.right, rect.bottom);
            export.PixelBounds = envelope;
            Int32 hDC = export.StartExporting();
            activeView.Output(hDC, (Int16)export.Resolution, ref rect, null, null);

            // Finish writing the export file and cleanup any intermediate files
            export.FinishExporting();
            export.Cleanup();

            return true;
        }
        #endregion

    }
}

