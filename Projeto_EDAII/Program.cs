using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using ProjetoGrafos.DataStructure;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Web;
using System.IO;
using System.Text;

namespace Projeto_EDAII
{
    class Program
    {
        public static Node nStart = new Node();
        public static Node nEnd = new Node();
        public static HashSet<string> names = new HashSet<string>();

        public static string baseUrl = ("https://pt.wikipedia.org");

        static void Main(string[] args)
        {
            do
            {
                Console.WriteLine("Digite a palavra de partida:");
                nStart.Name = RemoveAcentos(Console.ReadLine()).ToLower();

                Console.WriteLine("Digite a palavra de procura:");
                nEnd.Name = RemoveAcentos(Console.ReadLine()).ToLower();

                if (Exist())
                {
                    if (nStart.Name != nEnd.Name)
                    {
                        FindSolution();
                    }
                    else
                    {
                        Console.WriteLine("As palavras são iguais!!");
                    }
                }
                else
                {
                    Console.WriteLine("Uma das duas URLs não existe!!");
                }


                Console.WriteLine("-------------------------------------------------------------------------");
            } while (true);
        }

        public static bool Exist()
        {
            try
            {
                /// Testa a chamada das duas URLs para ver se existe, se cair no catch retorna false.
                HttpWebRequest requestStart = (HttpWebRequest)WebRequest.Create(nStart.Url);
                requestStart.Method = "GET";
                WebResponse responseStart = requestStart.GetResponse();
                StreamReader srStart = new StreamReader(responseStart.GetResponseStream(), Encoding.UTF8);

                HttpWebRequest requestEnd = (HttpWebRequest)WebRequest.Create(nEnd.Url);
                requestEnd.Method = "GET";
                WebResponse responseEnd = requestEnd.GetResponse();
                StreamReader srEnd = new StreamReader(responseEnd.GetResponseStream(), Encoding.UTF8);
            }
            catch (Exception)
            {

                return false;
            }

            return true;
        }

        public static void FindSolution()
        {
            try
            {
                Queue<Node> nQueue = new Queue<Node>();
                List<Node> nList = new List<Node>();

                nStart.Url = string.Concat("/wiki/", nStart.Name);
                nEnd.Url = string.Concat("/wiki/", nEnd.Name);
                nQueue.Enqueue(nStart);

                do
                {
                    Node n = nQueue.Dequeue();
                    GetSucessors(ref n);

                    if (n.Edges.Any(c => c.To.Name == nEnd.Name))
                    {
                        BuildAnswer(n);
                        break;
                    }

                    foreach (Edge e in n.Edges)
                    {
                        e.To.Visited = true;
                        e.To.Parent = n;
                        nQueue.Enqueue(e.To);
                    }

                } while (nQueue.Count > 0);
            }
            catch (Exception)
            {
                Console.WriteLine("Pagina não existe (404)");
            }
        }

        public static void BuildAnswer(Node n)
        {
            List<string> answers = new List<string>();

            while (n.Name != nStart.Name)
            {
                answers.Add(n.Url);
                n = n.Parent;
            }

            answers.Reverse();

            answers.Insert(0, nStart.Url.Replace(baseUrl, string.Empty));
            answers.Add(nEnd.Url.Replace(baseUrl, string.Empty));

            for (int i = 0; i < answers.Count; i++)
            {
                Console.WriteLine(string.Format("{0} - {1}{2}", i + 1, baseUrl, Uri.UnescapeDataString(answers[i])));
            }
        }

        public static string RemoveAcentos(string text)
        {
            string with = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûù";
            string without = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuu";

            for (int i = 0; i < with.Length; i++)
            {
                text = text.Replace(with[i].ToString(), without[i].ToString());
            }

            return text;
        }

        public static void GetSucessors(ref Node n)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();

                GetHtml(n);
                doc.LoadHtml(n.Html);

                //doc.DocumentNode.SelectSingleNode("/html/body")

                //doc.DocumentNode

                foreach (HtmlNode item in doc.DocumentNode.SelectSingleNode("/html/body").SelectNodes("//a/@href"))
                {
                    if (item != null)
                    {
                        if (item.Attributes["href"] != null)
                        {
                            if (!string.IsNullOrEmpty(item.Attributes["href"].Value)
                             && !string.IsNullOrEmpty(item.InnerHtml)
                             && item.Attributes["href"].Value.StartsWith("/wiki/")
                             && !item.InnerHtml.Contains("<"))
                            {
                                if (names.Add(item.InnerHtml))
                                {
                                    n.AddEdge(new Node()
                                    {
                                        Name = RemoveAcentos(item.InnerHtml).ToLower(),
                                        Url = item.Attributes["href"].Value
                                    });
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public static void GetHtml(Node n)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Concat(baseUrl, n.Url));
                request.Method = "GET";
                WebResponse response = request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                n.Html = sr.ReadToEnd();
            }
            catch
            {
                n.Url = Uri.UnescapeDataString(n.Url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Concat(baseUrl, n.Url));
                request.Method = "GET";
                WebResponse response = request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                n.Html = sr.ReadToEnd();
            }
        }
    }
}
