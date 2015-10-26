using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.ML;
using Emgu.CV.ML.MlEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace Search4Match.LoadSearcher
{
    class Load_L1BOF : IDisposable
    {
        private SVM topLayerSVM; private Matrix<float> topLayerDic;
        //private List<Image<Bgr, byte>> refImagesContainer = new List<Image<Bgr, byte>>();
        int j;
        private static int classNum = 0; //number of clusters/classes 
        private static SURFDetector _detector = new SURFDetector(500, false);
        private static BruteForceMatcher<float> _matcher = new BruteForceMatcher<float>(DistanceType.L2);
        private BOWKMeansTrainer bowTrainer = new BOWKMeansTrainer(classNum, new MCvTermCriteria(10, 0.01), 3,
        KMeansInitType.PPCenters);
        private BOWImgDescriptorExtractor<float> bowDe = new BOWImgDescriptorExtractor<float>(_detector, _matcher);

        private string prefix;
        public bool loaded = false;
        public Load_L1BOF(int classnum,string loc)
        {
            prefix = loc;
            topLayerDic = null;
            classNum = classnum;
            bowTrainer = new BOWKMeansTrainer(classNum, new MCvTermCriteria(10, 0.01), 3,
        KMeansInitType.PPCenters);

                IFormatter formatter = new BinaryFormatter();
                FileStream fs = File.OpenRead(loc + "obj\\dic.xml");
                topLayerDic = (Matrix<float>)formatter.Deserialize(fs);
                fs.Dispose();
                bowDe.SetVocabulary(topLayerDic);
                topLayerSVM = new SVM();
                topLayerSVM.Load(loc + "obj\\svm.xml");
                Console.WriteLine("Finished Loading L1 SVM.");
                loaded = true;
        }
        public float L1Predict(Image<Bgr, byte> refPic)
        {
            Image<Gray, byte> testImgGray = refPic.Convert<Gray, Byte>();
            VectorOfKeyPoint testKeyPoints = _detector.DetectKeyPointsRaw(testImgGray, null);
            BOWImgDescriptorExtractor<float> bowDe = new BOWImgDescriptorExtractor<float>(_detector, _matcher);
            bowDe.SetVocabulary(topLayerDic);
            Matrix<float> testBowDescriptor = bowDe.Compute(testImgGray, testKeyPoints);
            float result = topLayerSVM.Predict(testBowDescriptor);

            return result;
        }
        public void Dispose()
        {
            //Console.WriteLine("Training Complete, Result Saved.\n");
        }
    }
}
