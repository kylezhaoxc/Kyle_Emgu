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
    public class Load_BOFMaster
    {/// <summary>
    /// Generating a two-level Bag-Of-Feature SVM.
    /// </summary>
        #region params
        string prefix;
        float[] labels;
        List<string> trainingSet = new List<string>();
        int L1number = 0;
        List<Load_L2BOF> secondbof = new List<Load_L2BOF>();
        Load_L1BOF LayerOneBag;
        #endregion
        public Load_BOFMaster(bool isBack)
        {
            prefix = AppDomain.CurrentDomain.BaseDirectory + (isBack ? "img_back\\" : "img_forth\\");
            LayerOneBag = new Load_L1BOF(30,prefix);
            L1number = new DirectoryInfo(prefix).GetDirectories().Length - 1;
            labels = new float[L1number];
            //allocate uris
            for (int i = 0; i < L1number; i++)
            {
                labels[i] = i + 1;
                trainingSet.Add(prefix + "c" + labels[i]);
            }
            foreach (string src in trainingSet)
            {

                Load_L2BOF bag = new Load_L2BOF(50, src);
                secondbof.Add(bag);
                    bag.Dispose();
            }
        }
        public string BOFPredict(Image<Bgr, byte> target)
        {///<summary>tell which image in the refs are the most similar one to the input image.</summary>
            ///<param name="target">image to search</param>
            float classid = LayerOneBag.L1Predict(target);
            float id = secondbof[Convert.ToInt32(classid) - 1].L2Predict(target);
            string result = classid.ToString() + "-" + id.ToString();
            return result;
        }

    }
  
  
}
