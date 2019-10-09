using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRServerSDK
{
    class MasterManagement
    { 
        public static string readConfig()
        {
            try
            {
                return System.IO.File.ReadAllText("D:\\MasterConfig\\config.txt").Split(":".ToCharArray())[1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "File does not exist";
            }
            finally { }
        }
    }
}
