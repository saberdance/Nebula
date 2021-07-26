using System;
using System.Threading;
using System.Threading.Tasks;
using Nebula;
using tsubasa;

namespace CrawlerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("请输入token");
                return;
            }
            Console.WriteLine("开始爬取Etherscan智能合约");
            string filePath = "./address.csv";
            if (args.Length >= 2)
            {
                filePath = args[1];
            }
            Console.WriteLine($"地址文件:{filePath}");
            Console.WriteLine($"Token:{args[0]}");
            var crawler = new EsContractCrawler(filePath, args[0]);
            Task.Run(crawler.GetContractSource);
            while (true)
            {
                Thread.Sleep(5000);
            }
        }
    }
}
