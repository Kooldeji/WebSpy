using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WebSpy
{
    class ConversionClass
    {
        List<string> fileNames;
        void GetFiles()
        {
            //Load files from Corpus
            //Note that i am using a folder on my system as my corpus because we dont have a corpus yet.
            fileNames = Directory.GetFiles(@"C:\Users\Alamu Benjamin\Documents\Church\Baptist Hymnal Text", "*.*", SearchOption.AllDirectories)
            .Where(file => new string[] { ".txt", ".doc", ".docx" , ".pdf", ".ppt", ".ppts" , ".xls", ".xlsx", ".html", ".xml" } //Filter the files
            .Contains(Path.GetExtension(file)))
            .ToList();

            for (int i = 0; i < fileNames.Count; i++) //Prints all file names in the list
            {
                Console.WriteLine(fileNames[i]);
            }
            Console.ReadKey();
        }
        void ConvertFilesToText()
        {
            string folderName = "TextFiles"; //Create a folder to store the .txt equivalent of all documents
            Directory.CreateDirectory(folderName);
            for (int i = 0; i<fileNames.Count; i++)
            {
                //use tika to extract data and store inside variable fileData
                var fileData = "";
                string filePath = Path.Combine(folderName, (folderName + fileNames[i] + ".txt")); //Create .txt files
                File.Create(filePath);
                File.WriteAllText(filePath, fileData);
            }
        }

        public void GetPaths(string path)
        {
            Indexer ind = new Indexer();
            string dir = path;
            List<string> txtPaths = fileNames = Directory.GetFiles(dir, "*.txt").ToList();
            for(int i = 0; i < txtPaths.Count; i++)
            {
                ind.GenerateString(txtPaths[i]);
            }
        }
    }
}
