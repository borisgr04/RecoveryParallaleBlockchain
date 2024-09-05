using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexTypes;

public class BlockchainClient
{
    private BlockchainProxy blockchainProxy;

    public BlockchainClient(string rpcEndPoint, string contractAddress, string eventSignature)
    {
        this.blockchainProxy = new BlockchainProxy(rpcEndPoint, contractAddress, eventSignature);
    }

    public async Task<string> SearchLogs(string logId, ulong startBlock, ulong blockStep, int subPackageSize)
    {
        ulong endBlock = await blockchainProxy.GetLastBlockNumber();
        var totalBlocks = endBlock - startBlock + 1;
        var totalPackages = (ulong)Math.Ceiling((double)totalBlocks / blockStep);

        var cts = new CancellationTokenSource();

        for (ulong currentPackage = totalPackages; currentPackage > 0; currentPackage--)
        {
            var packageStartBlock = startBlock + (currentPackage - 1) * blockStep;
            var packageEndBlock = Math.Min(packageStartBlock + blockStep - 1, endBlock);

            var result = await SearchSubPackages(logId, packageStartBlock, packageEndBlock, subPackageSize, cts);
            if (!string.IsNullOrEmpty(result))
            {
                cts.Cancel(); // Cancel all other tasks
                return result;
            }
        }

        Console.WriteLine("No logs found for the given contract, event, and log ID in the specified block range.");
        return "";
    }

    private async Task<string?> SearchSubPackages(string logId, ulong packageStartBlock, ulong packageEndBlock, int subPackageSize, CancellationTokenSource cts)
    {
        var subPackageTasks = new List<Task<string?>>();
        var subPackageBlocks = (ulong)Math.Ceiling((double)(packageEndBlock - packageStartBlock + 1) / (ulong)subPackageSize);

        for (ulong currentSubPackage = 0; currentSubPackage < subPackageBlocks; currentSubPackage++)
        {
            var subPackageStartBlock = packageStartBlock + currentSubPackage * (ulong)subPackageSize;
            var subPackageEndBlock = Math.Min(subPackageStartBlock + (ulong)subPackageSize - 1, packageEndBlock);

            subPackageTasks.Add(CreateSubPackageTask(logId, subPackageStartBlock, subPackageEndBlock, subPackageSize, cts));
        }

        var results = await Task.WhenAll(subPackageTasks);

        foreach (var result in results)
        {
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
        }

        return null;
    }

    private Task<string?> CreateSubPackageTask(string logId, ulong subPackageStartBlock, ulong subPackageEndBlock, int subPackageSize, CancellationTokenSource cts)
    {
        return Task.Run(async () =>
        {
            if (cts.Token.IsCancellationRequested)
            {
                return null;
            }

            var result = await blockchainProxy.GetTransactionHashFromLog(logId, subPackageStartBlock, subPackageEndBlock, (ulong)subPackageSize);
            if (!string.IsNullOrEmpty(result))
            {
                cts.Cancel(); // Cancel all other tasks
            }
            return result;
        }, cts.Token);
    }
}
