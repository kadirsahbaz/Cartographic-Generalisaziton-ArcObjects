using System;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using KadirSahbaz;
using System.IO;
using System.Text;

namespace Genellestirme
{
    public class YerlesimAlaniOlustur : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public YerlesimAlaniOlustur()
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
            ILayer blokLayer = BinaGEN.Layer("Blok_Ada"),
                   yerlesimLayer = BinaGEN.Layer("Yerlesim_Ara"),
                   binaLayer = BinaGEN.Layer("Binalar_Yeni");
            IFeatureClass blokFClass = (blokLayer as IFeatureLayer).FeatureClass,
                          yerlesimFClass = (yerlesimLayer as IFeatureLayer).FeatureClass,
                          binaFClass = (binaLayer as IFeatureLayer).FeatureClass,
                          yogunYerlesimFClass;
            IFeatureCursor blokFCursor, yerlesimFCursor, yerlesimFCursor2, binaFCursor;
            IFeature blokFeature, yerlesimFeature, binaFeature, yogunYerlesimFeature;
            double blokAlan, yerlesimAlan, binaAlan;
            double maxAlanOrani = 0.85;
            String excelHucreDegeri;

            /* KONTROL İÇİN LOG DOSYASI */
            string excelDosyaYolu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\kontrol.xlsx";
            StreamWriter kontrolDosya = new StreamWriter(excelDosyaYolu, false, Encoding.Default);
            kontrolDosya.WriteLine("BLOK;YERLEŞİM ALANI;BİNA;ORAN;AÇIKLAMA");
            /*----------------------------*/

            // Yeni Yerlesim Alanını tutacak yeni bir featureclass oluşturuyoruz.
            string dosyaGDB = BinaGEN.TabakaKlasoruAl(blokLayer);
            IWorkspaceFactory wsf = new FileGDBWorkspaceFactoryClass();
            IWorkspace2 ws = (IWorkspace2)wsf.OpenFromFile(dosyaGDB, ArcMap.Application.hWnd);
            yogunYerlesimFClass = BinaGEN.YeniFeatureClassOlustur(ws, "Yerlesim_Yogun", map.SpatialReference, esriGeometryType.esriGeometryPolygon, null);

            // Yeni Yerlesim Alanını tutacak yeni bir tabaka oluşturuyoruz.
            IFeatureLayer yogunYerlesimFLayer = new FeatureLayerClass();
            yogunYerlesimFLayer.Name = "Yerlesim_Yogun";
            yogunYerlesimFLayer.FeatureClass = yogunYerlesimFClass;

            try
            {
                double yerlesimToplamAlan, binaToplamAlan, toplamAlan, oran;
                ISpatialFilter spatialFilter = new SpatialFilterClass();

                blokFCursor = blokFClass.Search(null, true);
                blokFeature = blokFCursor.NextFeature();
                while (blokFeature != null) //Blok'da teker teker dolaş
                {
                    blokAlan = (blokFeature.Shape as IArea).Area;
                    yerlesimToplamAlan = 0; binaToplamAlan = 0; toplamAlan = 0;
                    excelHucreDegeri = "_";

                    spatialFilter.Geometry = blokFeature.Shape;
                    spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    yerlesimFCursor = yerlesimFClass.Search(spatialFilter, true); //O anki Blok içindeki yerleşim alanlarını al.
                    yerlesimFeature = yerlesimFCursor.NextFeature();
                    if (yerlesimFeature == null)//Blokta yerleşim yeri yoksa sonraki bloğa geç
                    {
                        blokFeature = blokFCursor.NextFeature();
                        continue;
                    }

                    kontrolDosya.Write(blokFeature.OID.ToString() + ";");//Blok numarasını dosyaya yaz. KONTROL_İÇİN_____
                    while (yerlesimFeature != null)
                    {
                        excelHucreDegeri += yerlesimFeature.OID.ToString() + " ";
                        yerlesimAlan = (yerlesimFeature.Shape as IArea).Area;
                        yerlesimToplamAlan += yerlesimAlan; //yerleşim yeri alanlarını topla
                        yerlesimFeature = yerlesimFCursor.NextFeature();
                    }
                    kontrolDosya.Write(excelHucreDegeri + ";");//Yerleşim alanı numaralarını dosyaya yazyaz. KONTROL_İÇİN_____
                    binaFCursor = binaFClass.Search(spatialFilter, true); //O anki Blok içindeki binaları al.
                    binaFeature = binaFCursor.NextFeature();
                    if (binaFeature == null)
                    {
                        kontrolDosya.Write(";");
                    }

                    while (binaFeature != null)
                    {
                        excelHucreDegeri += binaFeature.OID.ToString() + " ";
                        binaAlan = (binaFeature.Shape as IArea).Area;
                        binaToplamAlan += binaAlan; //bina alanlarını topla
                        binaFeature = binaFCursor.NextFeature();
                    }

                    kontrolDosya.Write(excelHucreDegeri + ";");//Bina numaralarını dosyaya yaz. KONTROL_İÇİN_____
                    //Yeni tabakaya yerleşim yerlerini ekliyoruz
                    toplamAlan = binaToplamAlan + yerlesimToplamAlan;
                    if ((double)(toplamAlan / blokAlan) > maxAlanOrani)
                    {
                        //Tüm bloğu yerleşim alanı yap
                        yogunYerlesimFeature = yogunYerlesimFClass.CreateFeature();
                        yogunYerlesimFeature.Shape = blokFeature.Shape;
                        yogunYerlesimFeature.Store();

                        blokFeature = blokFCursor.NextFeature();

                        kontrolDosya.Write(((toplamAlan / blokAlan) * 100).ToString() + ";");//Alanlar oranını dosyaya yaz. KONTROL_İÇİN_____
                        kontrolDosya.WriteLine("BLOK YERLEŞİM ALANINA DÖNÜŞTÜRÜLDÜ");//açıklamayı dosyaya yaz. KONTROL_İÇİN_____
                        continue;
                    }
                    else
                    {
                        yerlesimFCursor2 = yerlesimFClass.Search(spatialFilter, true);
                        yerlesimFeature = yerlesimFCursor2.NextFeature();
                        while (yerlesimFeature != null)
                        {
                            yogunYerlesimFeature = yogunYerlesimFClass.CreateFeature();
                            ((IZAware)yerlesimFeature.Shape).ZAware = false; //Z değerini iptal ediyoruz. 
                            yogunYerlesimFeature.Shape = yerlesimFeature.Shape;
                            yogunYerlesimFeature.Store();

                            yerlesimFeature = yerlesimFCursor2.NextFeature();
                        }
                    }

                    oran = (toplamAlan / blokAlan) * 100;
                    kontrolDosya.WriteLine(oran.ToString() + ";;");//KONTROL_İÇİN_____                    
                    blokFeature = blokFCursor.NextFeature();
                }
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj(hata.Source, hata.Message);
            }
            finally
            {

                //Yeni yerlesim alanını haritaya ekliyoruz
                map.AddLayer(yogunYerlesimFLayer);

                //Yerleşim alanına eklenen binaları sil
                IGeoProcessor2 gp = new GeoProcessorClass();
                gp.AddOutputsToMap = true;
                gp.OverwriteOutput = true;
                gp.AddToolbox(@"C:\TEZ\MyTools.tbx");
                IVariantArray parametreler = new VarArrayClass();
                parametreler.Add("Binalar_Yeni");
                parametreler.Add("Yerlesim_Yogun");
                parametreler.Add(@"C:\TEZ\TezGDB.gdb\Binalar_Yeni2");
                gp.Execute("YerlesimAlanindakiBinalariSil", parametreler, null);

                // LOG dosyası ayarları
                kontrolDosya.Flush();
                kontrolDosya.Close();


                map.ClearSelection();
                mxDoc.UpdateContents();
                activeView.Refresh();
            }
            //BinaGEN.Mesaj(" ", "İşlem tamamlandı.");
        }

        protected override void OnUpdate()
        {
        }
    }
}
