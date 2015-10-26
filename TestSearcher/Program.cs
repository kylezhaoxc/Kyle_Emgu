using System;
using Search4Match;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Diagnostics;

namespace TestSearcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Hallway_Controller prep_controller = Hallway_Controller.GetPrepController(AppDomain.CurrentDomain.BaseDirectory + "img_back\\");
            prep_controller.Dispose();
            Hallway_Controller load_controller = Hallway_Controller.GetLoadController(AppDomain.CurrentDomain.BaseDirectory + "img_forth\\");
            string loc = string.Empty;
            //while (true)
            //{
            Console.WriteLine(load_controller.GetDirection(new Image<Bgr, byte>("D:\\1-test.jpg"), out loc) + "\t" + loc);
            //}
            Console.Read();
        }
    }
}
