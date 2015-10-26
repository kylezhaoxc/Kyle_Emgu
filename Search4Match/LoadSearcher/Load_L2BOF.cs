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
    class Load_L2BOF : IDisposable
    {
       
        string directory;
        private SVM SecondLayerSVM; private Matrix<float> SecondLayerDic;
        int j;
        private static int classNum; //number of clusters/classes 
        private static SURFDetector _detector = new SURFDetector(500, false);
        private static BruteForceMatcher<float> _matcher = new BruteForceMatcher<float>(DistanceType.L2);
        private BOWKMeansTrainer bowTrainer;
        private BOWImgDescriptorExtractor<float> bowDe = new BOWImgDescriptorExtractor<float>(_detector, _matcher);
        public bool loaded = false;
        public Load_L2BOF(int classnum, string src)
        {
            SecondLayerDic = null;
            directory = src;
            classNum = classnum;
            bowTrainer = new BOWKMeansTrainer(classNum, new MCvTermCriteria(10, 0.01), 3,
        KMeansInitType.PPCenters);
                IFormatter formatter = new BinaryFormatter();
                FileStream fs = File.OpenRead(directory + "\\obj\\dic.xml");
                SecondLayerDic = (Matrix<float>)formatter.Deserialize(fs);
                fs.Dispose();
                bowDe.SetVocabulary(SecondLayerDic);
                SecondLayerSVM = new SVM();
                SecondLayerSVM.Load(directory + "\\obj\\svm.xml");
                Console.WriteLine("Finished Loading L2 SVM.");loaded = true;
        }
        public float L2Predict(Image<Bgr, byte> refPic)
        {
            Image<Gray, byte> testImgGray = refPic.Convert<Gray, Byte>();
            VectorOfKeyPoint testKeyPoints = _detector.DetectKeyPointsRaw(testImgGray, null);
            Matrix<float> testBowDescriptor = bowDe.Compute(testImgGray, testKeyPoints);
            float result = SecondLayerSVM.Predict(testBowDescriptor);

            return result-1;
        }
        public void Dispose()
        {
            //Console.WriteLine("Finished Generating SVM for class\t" + this.directory.Substring(directory.Length - 1));
        }
    }
}
