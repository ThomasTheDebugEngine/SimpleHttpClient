using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpClient
{
    class Hypertext : IHypertext
    {
        public string CompileWebsite()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "src", "index.html");

            if(File.Exists(path))
            {
                string HtmlContent = File.ReadAllText(path,Encoding.UTF8);
                Console.WriteLine(HtmlContent);
                return HtmlContent;
            }
            else
            {
                return "404 NOT FOUND";
            }
        }
    }

    interface IHypertext
    {
        public string CompileWebsite();
    }
}
