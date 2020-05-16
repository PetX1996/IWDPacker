using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IWDPacker
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Packer packer = new Packer(args);

                //Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("********************************");
                Console.WriteLine("************ ERROR *************");
                Console.WriteLine("********************************");

                string error = string.Empty;
                error += e.Message + Environment.NewLine + e.StackTrace;
                if (e.InnerException != null)
                    error += Environment.NewLine + e.InnerException + Environment.NewLine + e.InnerException.StackTrace;

                Console.WriteLine(e.GetType().ToString());
                Console.WriteLine(error);
                Console.ReadKey();
            }
        }
    }
}
