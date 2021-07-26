using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsubasa;
using Sky.Data.Csv;

namespace Nebula
{
    public class AddressDescriberResolver : AbstractDataResolver<AddressDescriber>
    {
        public override AddressDescriber Deserialize(List<String> data)
        {
            return new AddressDescriber
            {
                Txhash = data[0],
                ContractAddress = data[1],
                ContractName = data[2]
            };
        }

        public override List<String> Serialize(AddressDescriber data)
        {
            return new List<String>
            {
                data.Txhash,
                data.ContractAddress,
                data.ContractName,
            };
        }
    }

    public class EsContractApi : IContractApi
    {
        private string ContractAddress { get; set; }
        private string ApiKey { get; set; }
        private WebHelper Web { get; set; } 

        public EsContractApi(string contractAddress, string apiKey , WebHelper helper)
        {
            ContractAddress = contractAddress;
            ApiKey = apiKey;
            Web = helper;
        }

        public async Task<string> Run()
        {
            string targetUri = @$"/api?module=contract&action=getsourcecode&address={ContractAddress}&apikey={ApiKey}";
            return await Web.GetRequest(targetUri);
        }
    }

    public class EsContractCrawler : IContractCrawler
    {
        public WebHelper Web = new WebHelper("https://api-cn.etherscan.com");
        private string ContractSaveFolder = "./contracts";
        private string AddressFilePath { get; set; }
        private string ApiKey = "5STSYWABCD8GP6X924GQSSVE8M83V6JM3H";

        public EsContractCrawler(string addressFilePath, string apiKey = null)
        {
            ApiKey = apiKey ?? ApiKey;
            AddressFilePath = addressFilePath;
            if (!Directory.Exists(ContractSaveFolder))
            {
                Directory.CreateDirectory(ContractSaveFolder);
            }
        }

        public async Task<bool> GetContractSource()
        {
            if (!File.Exists(AddressFilePath))
            {
                return false;
            }
            var dataRsolver = new AddressDescriberResolver();
            using (var reader = CsvReader<AddressDescriber>.Create(AddressFilePath,dataRsolver))
            {
                foreach (var addressInfo in reader)
                {
                    Logger.ConsoleLog($"爬取:[{addressInfo.ContractName}][{addressInfo.ContractAddress}],TxHash:[{addressInfo.Txhash}]");
                    bool succ = await GetOneContract(addressInfo);
                    Logger.ConsoleLog($"结果:{succ}");
                    await Task.Delay(200);
                }
            }
            return true;
        }

        private async Task<bool> GetOneContract(AddressDescriber contractAddress)
        {
            var api = new EsContractApi(contractAddress.ContractAddress, ApiKey, Web);
            string contract = await api.Run();
            if (!string.IsNullOrEmpty(contract))
            {
                return SaveTo(contract, contractAddress);
            }
            return false;
        }

        private bool SaveTo(string contract, AddressDescriber contractAddress)
        {
            try
            {
                string fileName = contractAddress.ContractName + ".txt";
                string filePath = ContractSaveFolder + "/" + fileName;
                if (File.Exists(filePath))
                {
                    fileName = contractAddress.ContractName + "_" + UtilFunc.MD5String(DateTime.Now.ToString()) + ".txt";
                }
                using (StreamWriter sw = new(filePath,false,Encoding.UTF8,4096000))
                {
                    sw.WriteLine("//Address: " + contractAddress.ContractAddress);
                    sw.WriteLine("//TxHash: " + contractAddress.Txhash);
                    sw.WriteLine("//----------------------------------------contract-----------------------------------------------");
                    string final = contract.Replace(@"\r\n", Environment.NewLine)
                        .Replace(@"\\r\\n", Environment.NewLine)
                        .Replace(@"\n", Environment.NewLine);
                    sw.WriteLine(final);
                }
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }

        public void SetContractFolder(string contractFolder)
        {
            ContractSaveFolder = contractFolder;
        }
    }
}
