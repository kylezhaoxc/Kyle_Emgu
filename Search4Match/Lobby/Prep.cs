using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV.ML;
using Emgu.CV.ML.MlEnum;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Search4Match.Lobby
{
    public class Prep
    {
        private string dir;
        private SVM SVM; private Matrix<float> Dic;
        private List<Image<Bgr, byte>> images;
        private static int classNum=30;
        Matrix<float> labels;
        int j = 0;
        private static SURFDetector _detector = new SURFDetector(500, false);
        private static BruteForceMatcher<float> _matcher = new BruteForceMatcher<float>(DistanceType.L2);
        private BOWKMeansTrainer bowTrainer = new BOWKMeansTrainer(classNum, new MCvTermCriteria(10, 0.01), 3,
        KMeansInitType.PPCenters);
        private BOWImgDescriptorExtractor<float> bowDe = new BOWImgDescriptorExtractor<float>(_detector, _matcher);
        private Matrix<float> trainingDescriptors;
        public Prep(List<Image<Bgr, byte>> imgs,string directory)
        {
            dir = directory;
            images = imgs;
            bowTrainer = new BOWKMeansTrainer(classNum, new MCvTermCriteria(10, 0.01), 3,
        KMeansInitType.PPCenters);
            labels = new Matrix<float>(images.Count, 1);

            foreach (Image<Bgr, byte> img in images)
            {
                Extract(img);
            }
            MakeDic();
            double i = 1;
            foreach (Image<Bgr, byte> img in images)
            {
                int label_value = Convert.ToInt32(Math.Floor(i));
                MakeDescriptors(img,label_value);
                i += 0.5;
            }
            Save();
        }
        //extract descriptors
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
        //make Dictionarys
        private void MakeDic()
        {
            l1:
            try
            {
                Dic = bowTrainer.Cluster();
                bowDe.SetVocabulary(Dic);
                trainingDescriptors = new Matrix<float>(images.Count, classNum);
            }
            catch (Exception e)
            {
                int int_classnum = classNum;
                int_classnum++;
                trainingDescriptors = new Matrix<float>(images.Count, int_classnum);
                bowTrainer = new BOWKMeansTrainer(int_classnum, new MCvTermCriteria(10, 0.01), 3,
        KMeansInitType.PPCenters);
                goto l1;
            }
            
        }
        //assign labels
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
        //train and save
        public void Save()
        {
            SVM = new SVM();
            SVMParams p = new SVMParams();
            p.KernelType = SVM_KERNEL_TYPE.LINEAR;
            p.SVMType = SVM_TYPE.C_SVC;
            p.C = 1;
            p.TermCrit = new MCvTermCriteria(100, 0.00001);
            SVM.Train(trainingDescriptors, labels, null, null, p);
            IFormatter formatter = new BinaryFormatter();
            Stream fs = File.OpenWrite(dir+ "\\Dic");
            formatter.Serialize(fs, Dic);

            fs.Dispose();
            SVM.Save(dir+"\\SVM.xml");

        }
    }
    }

