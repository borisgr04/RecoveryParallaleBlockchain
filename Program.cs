using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Nethereum.ABI.Util;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using System.Diagnostics;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main(string[] args)
    {

        var builder = new ConfigurationBuilder()
          .AddUserSecrets<Program>();

        var configuration = builder.Build();

        var rpcEndPoint = configuration["RpcEndPoint"];
        var contractAddress = configuration["ContractAddress"];
        var eventSignature = configuration["EventSignature"];


        var logIds = new List<string>()
        {
            "0xf1bfcd7d333dc788c920f32ec6e9390b684820d1562771d71bbb8839e77893c3",
            "0x3609d690ec747004ccf0b173de01ef76061ae4d495c99fcd286f1739cb20080d",
            "0x4dafd439741a9c9b0dd53c73f149efa25be2342c671cff137e76c4c401cb01cc",
            "0x5ab3c92541c2f40ab87ddc483c6a1ad017a5be87d4cc25b54eca4817c95daad0",
            "0x0570ddda3f33cc2d9530369b1657b46550a98f2560d579fd13dcd495967efccb"
        };

        Console.WriteLine("Busqueda Paralela con cancelación");
        var client = new BlockchainClient(rpcEndPoint, contractAddress, eventSignature);
        foreach (var specificLogId in logIds)
        {
            var watch = new Stopwatch();
            watch.Start();
            var result = await client.SearchLogs(specificLogId, 0, 2000000, 1000000);
            Console.ForegroundColor = ConsoleColor.Green;
            if (!string.IsNullOrEmpty(result))
            {
                Console.WriteLine(result);
            }
            watch.Stop();
            string respuesta = $"{specificLogId} time: {watch.ElapsedMilliseconds}";
            Console.WriteLine(respuesta);
            Console.ResetColor();
        }

        Console.WriteLine("Presionar tecla para continuar");
        Console.ReadKey();
       
        Console.WriteLine("Busqueda Paralela sin cancelación");
        var client2 = new BlockchainClient2(rpcEndPoint, contractAddress, eventSignature);
        foreach (var specificLogId in logIds)
        {
            var watch = new Stopwatch();
            watch.Start();
            var result = await client2.SearchLogs(specificLogId, 0, 2000000, 1000000);
            Console.ForegroundColor = ConsoleColor.Green;
            if (!string.IsNullOrEmpty(result))
            {
                Console.WriteLine(result);
            }
            watch.Stop();
            string respuesta = $"{specificLogId} time: {watch.ElapsedMilliseconds}";
            Console.WriteLine(respuesta);
            Console.ResetColor();
        }

        Console.WriteLine("Presionar tecla para continuar");
        Console.ReadKey();


        Console.WriteLine("Busqueda Invertida simple");
        var blockchainProxy = new BlockchainProxy(rpcEndPoint, contractAddress, eventSignature);
        foreach (var specificLogId in logIds)
        {
            var watch = new Stopwatch();
            watch.Start();
            var endBlock = await blockchainProxy.GetLastBlockNumber();
            var result = await blockchainProxy.GetTransactionHashFromLogInvertido(specificLogId, 0, endBlock,  1000000);
            Console.ForegroundColor = ConsoleColor.Green;
            if (!string.IsNullOrEmpty(result))
            {
                Console.WriteLine(result);
            }
            watch.Stop();
            string respuesta = $"{specificLogId} time: {watch.ElapsedMilliseconds}";
            Console.WriteLine(respuesta);
            Console.ResetColor();
        }




        Console.ReadKey();
    }
}





