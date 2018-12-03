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
        public static Graph graph = new Graph();
        public static Node nStart = new Node();
        public static Node nEnd = new Node();
        public static HashSet<string> names = new HashSet<string>();

        public static string baseUrl = ("https://pt.wikipedia.org");

        static void Main(string[] args)
        {

            Console.WriteLine("Digite a palavra de partida:");
            nStart.Name = Console.ReadLine();

            Console.WriteLine("Digite a palavra de procura:");
            nEnd.Name = Console.ReadLine();

            if (!TargetFound(nStart))
            {
                FindSolution();
            }
            else
            {
                Console.WriteLine("As palavras são iguais!!");
            }

            Console.ReadKey();
        }

        public static bool TargetFound(Node n)
        {
            if (RemoveAcentos(n.Name.ToLower()).Equals(RemoveAcentos(nEnd.Name.ToLower())))
            {
                return true;
            }

            return false;
        }

        public static void FindSolution()
        {
            try
            {
                Queue<Node> nQueue = new Queue<Node>();
                List<Node> nList = new List<Node>();

                nStart.Url = string.Concat("/wiki/", nStart.Name);
                nQueue.Enqueue(nStart);

                do
                {
                    Node n = nQueue.Dequeue();
                    GetSucessors(ref n);

                    if (n.Visited != true)
                    {
                        if (n.Edges.Any(c => RemoveAcentos(c.To.Name).ToLower() == RemoveAcentos(nEnd.Name).ToLower()))
                        {
                            break;
                        }

                        foreach (Edge e in n.Edges)
                        {
                            if (e.To.Visited != true)
                            {
                                e.To.Visited = true;
                                e.To.Parent = n;
                                nQueue.Enqueue(e.To);
                            }
                        }
                    }

                } while (nQueue.Count > 0);

            }
            catch (Exception)
            {
                Console.WriteLine("Pagina não existe (404)");

                throw;
            }
        }

        public static string RemoveAcentos(string text)
        {
            string with = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
            string without = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

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
                                if (names.Add(item.Attributes["href"].Value))
                                {
                                    n.AddEdge(new Node()
                                    {
                                        Name = item.InnerHtml,
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
                n.Html = Uri.UnescapeDataString(sr.ReadToEnd());
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
