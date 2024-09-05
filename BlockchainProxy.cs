using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using System.Diagnostics;

public class BlockchainProxy 
{
    private Web3 web3;
    private string rpcEndPoint ;
    private string contractAddress ; // Reemplaza con la dirección del contrato
    private string eventSignature; // Reemplaza con la firma del evento (hash del evento)
    public BlockchainProxy(string rpcEndPoint,string contractAddress, string eventSignature)
    {
        this.contractAddress = contractAddress;
        this.eventSignature = eventSignature;
        this.rpcEndPoint = rpcEndPoint;
        this.web3 = new Web3(new TrueRpcClient(new Uri(this.rpcEndPoint)));
    }

    public async Task<bool> IsConnected() 
    {
        // Verificar conexión
        bool isConnected = await this.web3.Eth.Blocks.GetBlockNumber.SendRequestAsync() != null;
        Console.WriteLine(isConnected ? "Conectado a Ethereum" : "No se pudo conectar a Ethereum");
        return isConnected;
    }

    public async Task<ulong> GetLastBlockNumber() 
    {
        var endBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync(); // Bloque final
        return (ulong)endBlock.Value;
    }
    public async Task<string> GetTransactionHashFromLog(string logId, ulong startBlock, ulong endBlock, ulong blockStep)
    {
        var watch = new Stopwatch();
        watch.Start();
        for (ulong currentStartBlock = startBlock; currentStartBlock <= endBlock; currentStartBlock += blockStep)
        {
            var currentEndBlock = Math.Min(currentStartBlock + blockStep - 1, endBlock);
            var filterInput = new NewFilterInput
            {
                FromBlock = new BlockParameter(currentStartBlock),
                ToBlock = new BlockParameter(currentEndBlock),
                Address = new[] { this.contractAddress },
                Topics = new[] { this.eventSignature, "0x0000000000000000000000000000000000000000000000000000000000000001", "0x0000000000000000000000000000000000000000000000000000000000000002", logId }
            };
            var watchItem = new Stopwatch();
            watchItem.Start();
            var logs = await this.web3.Eth.Filters.GetLogs.SendRequestAsync(filterInput);
            watchItem.Stop();
            Console.WriteLine($"Request Blockchain => Bloques {currentStartBlock} - {currentEndBlock} Time por segmento {watchItem.ElapsedMilliseconds}");
            watchItem.Stop();

            if (logs.Count() > 0)
            {
                watch.Stop();
                string respuesta = $" {logId};{logs[0].TransactionHash};{logs[0].BlockNumber};{watch.ElapsedMilliseconds}";
                Console.WriteLine($"Request Blockchain =>{respuesta}");
                return respuesta; // Termina la búsqueda después de encontrar el primer log
            }
        }
        watch.Stop();
        Console.WriteLine($"No logs found for the given contract, event, and log ID in the specified block range. time {watch.ElapsedMilliseconds}");
        return "";
    }

    public async Task<string> GetTransactionHashFromLogInvertido(string logId, ulong startBlock, ulong endBlock, ulong blockStep)
    {
        
        var watch = new Stopwatch();
        watch.Start();
        Console.WriteLine($"Id;Transaction Hash;Bloque;Time");
        for (ulong currentStartBlock = endBlock; currentStartBlock >= startBlock; currentStartBlock -= blockStep)
        {
            if (currentStartBlock < startBlock)
            {
                break;
            }

            var currentEndBlock = Math.Min(currentStartBlock + blockStep - 1, endBlock);
            
            if (currentEndBlock == currentStartBlock && endBlock!= startBlock) continue;

            var filterInput = new NewFilterInput
            {
                FromBlock = new BlockParameter(currentStartBlock),
                ToBlock = new BlockParameter(currentEndBlock),
                Address = new[] { this.contractAddress },
                Topics = new[] { this.eventSignature, "0x0000000000000000000000000000000000000000000000000000000000000001", "0x0000000000000000000000000000000000000000000000000000000000000002", logId }
            };
            var watchItem = new Stopwatch();
            watchItem.Start();
            var logs = await web3.Eth.Filters.GetLogs.SendRequestAsync(filterInput);
            watchItem.Stop();
            Console.WriteLine($"Bloques {currentStartBlock} - {currentEndBlock} Time por segmento {watchItem.ElapsedMilliseconds}");
            watchItem.Stop();

            if (logs.Count() > 0)
            {
                watch.Stop();
                string respuesta = $"{logId};{logs[0].TransactionHash};{logs[0].BlockNumber};{watch.ElapsedMilliseconds}";
                Console.WriteLine(respuesta);
                return respuesta; // Termina la búsqueda después de encontrar el primer log
            }
        }
        watch.Stop();
        Console.WriteLine($"No logs found for the given contract, event, and log ID in the specified block range. time {watch.ElapsedMilliseconds}");
        return "";
    }

}


