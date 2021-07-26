using System;
using System.Threading.Tasks;

namespace Nebula
{
    public class AddressDescriber
    {
        public string Txhash { get; set; }
        public string ContractAddress { get; set; }
        public string ContractName { get; set; }
    }

    public interface IContractApi
    {
       Task<string> Run();
    }

    public interface IContractCrawler
    {
        Task<bool> GetContractSource();
        void SetContractFolder(string contractFolder);
    }
}
