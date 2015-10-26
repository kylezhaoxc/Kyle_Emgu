﻿using Emgu.CV;
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
namespace Search4Match.PrepSearcher
{
    class Prep_L1BOF : IDisposable
    {
        private SVM topLayerSVM; private Matrix<float> topLayerDic;
        //private List<Image<Bgr, byte>> refImagesContainer = new List<Image<Bgr, byte>>();
        int TopLayerNum = 0;
        int j;
        private static int classNum = 0; //number of clusters/classes 
        Matrix<float> labels;
        private static SURFDetector _detector = new SURFDetector(500, false);
        private static BruteForceMatcher<float> _matcher = new BruteForceMatcher<float>(DistanceType.L2);
        private BOWKMeansTrainer bowTrainer = new BOWKMeansTrainer(classNum, new MCvTermCriteria(10, 0.01), 3,
        KMeansInitType.PPCenters);
        private BOWImgDescriptorExtractor<float> bowDe = new BOWImgDescriptorExtractor<float>(_detector, _matcher);
        private Matrix<float> trainingDescriptors;
        private string directory;
        public Prep_L1BOF(int classnum,string loc)
        {
            directory = loc;
            topLayerDic = null;
            classNum = classnum;
            bowTrainer = new BOWKMeansTrainer(classNum, new MCvTermCriteria(10, 0.01), 3,
        KMeansInitType.PPCenters);
        }
        public void TrainByClass(List<string> dirs, float[] classes)
        {
            foreach (string dir in dirs)
            {
                foreach (FileInfo file in new DirectoryInfo(dir).GetFiles())
                {
                    TopLayerNum++;
                }

            }
            labels = new Matrix<float>(TopLayerNum, 1);
            trainingDescriptors = new Matrix<float>(TopLayerNum, classNum);
            j = 0;
            foreach (string dir in dirs)
            {
                foreach (FileInfo file in new DirectoryInfo(dir).GetFiles())
                {
                    Extract(new Image<Bgr, byte>(file.FullName));
                }


            }
            MakeDic();
            int i = 0; ; j = 0;
            foreach (string dir in dirs)
            {
                foreach (FileInfo file in new DirectoryInfo(dir).GetFiles())
                {
                    MakeDescriptors(new Image<Bgr, byte>(file.FullName), classes[i]);
                }
                i++;

            }
            Console.WriteLine("Finished Generating Layer one SVM..........Result Saved!");
        }
        public void Save()
        {
            topLayerSVM = new SVM();
            SVMParams p = new SVMParams();
            p.KernelType = SVM_KERNEL_TYPE.LINEAR;
            p.SVMType = SVM_TYPE.C_SVC;
            p.C = 1;
            p.TermCrit = new MCvTermCriteria(100, 0.00001);
            topLayerSVM.Train(trainingDescriptors, labels, null, null, p);
            IFormatter formatter = new BinaryFormatter();
            Stream fs = File.OpenWrite(directory + "obj\\dic.xml");
            formatter.Serialize(fs, topLayerDic);

            fs.Dispose();
            topLayerSVM.Save(directory + "obj\\svm.xml");

        }
        private void Extract(Image<Bgr, byte> newRefPic)
        {

            //refImagesContainer.Add(newRefPic);
            bowDe = new BOWImgDescriptorExtractor<float>(_detector, _matcher);


            using (Image<Gray, byte> modelGray = newRefPic.Convert<Gray, Byte>())
            //Detect SURF key points from images 
            using (VectorOfKeyPoint modelKeyPoints = _detector.DetectKeyPointsRaw(modelGray, null))
            //Compute detected SURF key points & extract modelDescriptors 
            using (
            Matrix<float> modelDescriptors = _detector.ComputeDescriptorsRaw(modelGray, null, modelKeyPoints)
            )
            {
                //Add the extracted BoW modelDescriptors into BOW trainer 
                bowTrainer.Add(modelDescriptors);
            }
        }
        private void MakeDic()
        {
            topLayerDic = bowTrainer.Cluster();
            bowDe.SetVocabulary(topLayerDic);
        }
        private void MakeDescriptors(Image<Bgr, byte> newRefPic, float x)
        {
            using (Image<Gray, byte> modelGray = newRefPic.Convert<Gray, Byte>())
            using (VectorOfKeyPoint modelKeyPoints = _detector.DetectKeyPointsRaw(modelGray, null))
            using (Matrix<float> modelBowDescriptor = bowDe.Compute(modelGray, modelKeyPoints))
            {
                //To merge all modelBOWDescriptor into single trainingDescriptors 
                for (int i = 0; i < trainingDescriptors.Cols; i++)
                {
                    trainingDescriptors.Data[j, i] = modelBowDescriptor.Data[0, i];
                }
                labels.Data[j, 0] = x;
                j++;
            }
        }

        //private float LoadAndPredict(Image<Bgr, byte> refPic)
        //{
        //    Image<Gray, byte> testImgGray = refPic.Convert<Gray, byte>();
        //    VectorOfKeyPoint testKeyPoints = _detector.DetectKeyPointsRaw(testImgGray, null);
        //    BOWImgDescriptorExtractor<float> bowDe = new BOWImgDescriptorExtractor<float>(_detector, _matcher);
        //    IFormatter formatter = new BinaryFormatter();
        //    FileStream fs = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "obj\\dic.xml");
        //    topLayerDic = (Matrix<float>)formatter.Deserialize(fs);
        //    fs.Dispose();
        //    bowDe.SetVocabulary(topLayerDic);
        //    topLayerSVM = new SVM();
        //    topLayerSVM.Load(AppDomain.CurrentDomain.BaseDirectory + "\\obj\\svm.xml");
        //    Matrix<float> testBowDescriptor = bowDe.Compute(testImgGray, testKeyPoints);
        //    float result = topLayerSVM.Predict(testBowDescriptor);
        //    loaded = true;
        //    return result;
        //}
        //need to add method for a serie of image , dividing them into classes , each class contains 2 images.
        private float ImmediatPredict(Image<Bgr, byte> refPic)
        {
            Image<Gray, byte> testImgGray = refPic.Convert<Gray, Byte>();
            VectorOfKeyPoint testKeyPoints = _detector.DetectKeyPointsRaw(testImgGray, null);
            BOWImgDescriptorExtractor<float> bowDe = new BOWImgDescriptorExtractor<float>(_detector, _matcher);
            bowDe.SetVocabulary(topLayerDic);
            Matrix<float> testBowDescriptor = bowDe.Compute(testImgGray, testKeyPoints);
            float result = topLayerSVM.Predict(testBowDescriptor);

            return result;
        }
        public float L1Predict(Image<Bgr, byte> refPic)
        {
            ////if (loaded) 
            return ImmediatPredict(refPic);
            //return LoadAndPredict(refPic);
        }

        public void Dispose()
        {
            //Console.WriteLine("Training Complete, Result Saved.\n");
        }
    }
}
