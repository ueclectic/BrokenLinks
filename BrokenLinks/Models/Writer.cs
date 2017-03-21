using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BrokenLinks.Models
{
    public class Writer
    {
        public void WriteToFile(string item)
        {
            //string fileName = "log.txt";
            //if (!File.Exists(fileName))
            //{
            //    var result = MessageBox.Show($"Create file '{Directory.GetCurrentDirectory()}\\{fileName}?", "Parser", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            //    if (result != MessageBoxResult.OK)
            //        throw new Exception("Error: can't create a log file");

            //    using (StreamWriter sw = File.CreateText(fileName))
            //    { }
            //}

            //using (StreamWriter sw = new StreamWriter(fileName, true))
            //{
            //    sw.WriteLine(item);
            //}
        }
    }
}
