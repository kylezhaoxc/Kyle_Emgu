using System;
using System.Collections.Generic;
using System.IO;

namespace Search4Match
{
    public class FrameProcessor
    {
        private static FrameProcessor _handler;
        private static List<int[]> refFrameId;
        private static string prefix = AppDomain.CurrentDomain.BaseDirectory + "img";
        public static FrameProcessor GetProcessor(int framecount,string temp)
        {

            if (_handler != null) return _handler;
            else
            {
                _handler = new FrameProcessor();
                prefix += "_"+temp + "\\";
                refFrameId = OrganizeFrames(framecount);
                MakeDirs();
                return _handler;
            }
        }

        private static void MakeDirs()
        {
            foreach (DirectoryInfo dir in new DirectoryInfo(prefix).GetDirectories())
            {
                if(dir.Name!="obj")Directory.Delete(dir.FullName, true);
            }
            int a = 1;
            while (a <= refFrameId.Count)
            {
                string cur_class = a + "-";
                Directory.CreateDirectory(prefix + "c" + a);
                Directory.CreateDirectory(prefix + "c" + a+"\\obj");
                int i = 0;
                foreach (int ind in refFrameId[2 - a])
                {
                    File.Move(prefix + ind + ".jpg", prefix + "c" + a + "\\" + cur_class + (i) + ".jpg");
                    i++;
                }
                a++;
            }
            FileHelper.prefix = prefix;
            FileHelper.RemoveFile();
        }
        private static List<int[]> OrganizeFrames(int framecount)
        {
            List<int[]> index_list = new List<int[]>();
            //I hope to have 2 classes, and each class has 10 reference image inside.
            int trainingSetClassNum = 2;
            int trainingSetFrameCount = 10;
            //So the input reference frames shall be diveded into two divisions,I need to find out the leap.
            double frameCountInClass = framecount / trainingSetClassNum;
            double leap = frameCountInClass / trainingSetFrameCount;

            //start to establish the mapping from input index to training set index.
            int currentclass = trainingSetClassNum;
            while (currentclass > 0)
            {
                int[] temp = new int[10];
                for (int i = trainingSetFrameCount-1; i >= 0; i--)
                {
                    temp[i] = Convert.ToInt32(framecount - (trainingSetClassNum - currentclass) * frameCountInClass - (trainingSetFrameCount-1 - i) * leap);
                }
                index_list.Add(temp);
                currentclass--;
            }
            return index_list;
        }
    }
}
