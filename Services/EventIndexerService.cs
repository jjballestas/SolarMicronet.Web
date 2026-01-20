using System.Numerics;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.FunctionEncoding.Attributes;
using SolarMicronet.Web.Models;

namespace SolarMicronet.Web.Services;

public interface IEventIndexerService
{
    Task<List<EnergyGeneratedEvent>> GetEnergyGeneratedEventsAsync(string? participant = null, ulong fromBlock = 0, ulong? toBlock = null);
    Task<List<EnergyConsumedEvent>> GetEnergyConsumedEventsAsync(string? participant = null, ulong fromBlock = 0, ulong? toBlock = null);
    Task<List<TransferEvent>> GetTransferEventsAsync(string? from = null, string? to = null, ulong fromBlock = 0, ulong? toBlock = null);
}

public class EventIndexerService : IEventIndexerService
{
    private readonly ILogger<EventIndexerService> _logger;
    private readonly Web3 _web3;
    private readonly Contract _microgridContract;
    private readonly Contract _energonContract;

    public EventIndexerService(ILogger<EventIndexerService> logger)
    {
        _logger = logger;
        _web3 = new Web3(BlockchainConfig.RPC_URL);
        _microgridContract = _web3.Eth.GetContract(ContractABIs.MicrogridManagerABI, BlockchainConfig.MICROGRID_MANAGER_ADDRESS);
        _energonContract = _web3.Eth.GetContract(ContractABIs.EnergonTokenABI, BlockchainConfig.ENERGON_TOKEN_ADDRESS);
    }

    public async Task<List<EnergyGeneratedEvent>> GetEnergyGeneratedEventsAsync(string? participant = null, ulong fromBlock = 0, ulong? toBlock = null)
    {
        var events = new List<EnergyGeneratedEvent>();

        try
        {
            var eventHandler = _microgridContract.GetEvent<EnergyGeneratedEventDTO>();
            var filterInput = eventHandler.CreateFilterInput(
                new BlockParameter(fromBlock),
                toBlock.HasValue ? new BlockParameter(toBlock.Value) : BlockParameter.CreateLatest()
            );

            // El tipo ya está inferido de GetEvent<T>
            var logs = await eventHandler.GetAllChangesAsync(filterInput);

            foreach (var log in logs)
            {
                var decoded = log.Event;
                if (decoded == null) continue;

                // Filtrar por participante si se especifica
                if (!string.IsNullOrEmpty(participant) && 
                    !string.Equals(decoded.Participant, participant, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var block = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(
                    new BlockParameter(log.Log.BlockNumber));

                events.Add(new EnergyGeneratedEvent
                {
                    Participant = decoded.Participant,
                    Amount = decoded.Amount,
                    Meter = decoded.Meter,
                    ToParticipant = decoded.ToParticipant,
                    ToFund = decoded.ToFund,
                    BlockNumber = log.Log.BlockNumber.Value,
                    TransactionHash = log.Log.TransactionHash ?? string.Empty,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value).DateTime
                });
            }

            return events.OrderByDescending(e => e.BlockNumber).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting EnergyGenerated events");
            return events;
        }
    }

    public async Task<List<EnergyConsumedEvent>> GetEnergyConsumedEventsAsync(string? participant = null, ulong fromBlock = 0, ulong? toBlock = null)
    {
        var events = new List<EnergyConsumedEvent>();

        try
        {
            var eventHandler = _microgridContract.GetEvent<EnergyConsumedEventDTO>();
            var filterInput = eventHandler.CreateFilterInput(
                new BlockParameter(fromBlock),
                toBlock.HasValue ? new BlockParameter(toBlock.Value) : BlockParameter.CreateLatest()
            );

            // El tipo ya está inferido de GetEvent<T>
            var logs = await eventHandler.GetAllChangesAsync(filterInput);

            foreach (var log in logs)
            {
                var decoded = log.Event;
                if (decoded == null) continue;

                if (!string.IsNullOrEmpty(participant) && 
                    !string.Equals(decoded.Participant, participant, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var block = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(
                    new BlockParameter(log.Log.BlockNumber));

                events.Add(new EnergyConsumedEvent
                {
                    Participant = decoded.Participant,
                    Amount = decoded.Amount,
                    Meter = decoded.Meter,
                    BlockNumber = log.Log.BlockNumber.Value,
                    TransactionHash = log.Log.TransactionHash ?? string.Empty,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value).DateTime
                });
            }

            return events.OrderByDescending(e => e.BlockNumber).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting EnergyConsumed events");
            return events;
        }
    }

    public async Task<List<TransferEvent>> GetTransferEventsAsync(string? from = null, string? to = null, ulong fromBlock = 0, ulong? toBlock = null)
    {
        var events = new List<TransferEvent>();

        try
        {
            var eventHandler = _energonContract.GetEvent<TransferEventDTO>();
            var filterInput = eventHandler.CreateFilterInput(
                new BlockParameter(fromBlock),
                toBlock.HasValue ? new BlockParameter(toBlock.Value) : BlockParameter.CreateLatest()
            );

            // El tipo ya está inferido de GetEvent<T>
            var logs = await eventHandler.GetAllChangesAsync(filterInput);

            foreach (var log in logs)
            {
                var decoded = log.Event;
                if (decoded == null) continue;

                // Filtrar por from/to si se especifica
                if (!string.IsNullOrEmpty(from) && 
                    !string.Equals(decoded.From, from, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(to) && 
                    !string.Equals(decoded.To, to, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var block = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(
                    new BlockParameter(log.Log.BlockNumber));

                events.Add(new TransferEvent
                {
                    From = decoded.From,
                    To = decoded.To,
                    Value = decoded.Value,
                    BlockNumber = log.Log.BlockNumber.Value,
                    TransactionHash = log.Log.TransactionHash ?? string.Empty,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value).DateTime
                });
            }

            return events.OrderByDescending(e => e.BlockNumber).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Transfer events");
            return events;
        }
    }

    // DTOs para decodificación de eventos
    [Event("EnergyGenerated")]
    public class EnergyGeneratedEventDTO : IEventDTO
    {
        [Parameter("address", "participant", 1, true)]
        public string Participant { get; set; } = string.Empty;

        [Parameter("uint256", "amount", 2, false)]
        public BigInteger Amount { get; set; }

        [Parameter("address", "meter", 3, true)]
        public string Meter { get; set; } = string.Empty;

        [Parameter("uint256", "toParticipant", 4, false)]
        public BigInteger ToParticipant { get; set; }

        [Parameter("uint256", "toFund", 5, false)]
        public BigInteger ToFund { get; set; }
    }

    [Event("EnergyConsumed")]
    public class EnergyConsumedEventDTO : IEventDTO
    {
        [Parameter("address", "participant", 1, true)]
        public string Participant { get; set; } = string.Empty;

        [Parameter("uint256", "amount", 2, false)]
        public BigInteger Amount { get; set; }

        [Parameter("address", "meter", 3, true)]
        public string Meter { get; set; } = string.Empty;
    }

    [Event("Transfer")]
    public class TransferEventDTO : IEventDTO
    {
        [Parameter("address", "from", 1, true)]
        public string From { get; set; } = string.Empty;

        [Parameter("address", "to", 2, true)]
        public string To { get; set; } = string.Empty;

        [Parameter("uint256", "value", 3, false)]
        public BigInteger Value { get; set; }
    }
}
