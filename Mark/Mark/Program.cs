using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mark
{
    class Program
    {
        static void Main(string[] args)
        {

        }
    }

    public interface IMarkDownProcessor
    {
        string TranslateToHtml(string text);
    }
}
