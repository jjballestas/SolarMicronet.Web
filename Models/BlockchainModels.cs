using System.Numerics;

namespace SolarMicronet.Web.Models;

/// <summary>
/// Tipo de participante en la microred
/// </summary>
public enum ParticipantType
{
    Consumer = 0,  // Solo puede consumir y transferir
    Prosumer = 1   // Puede generar, consumir y transferir
}

/// <summary>
/// Estado de una actividad comunitaria
/// </summary>
public enum ActivityState
{
    Available = 0,      // Creada, disponible para reclamar
    Claimed = 1,        // Reclamada por un ejecutor
    InValidation = 2,   // Completada, en proceso de validación
    Paid = 3,           // Pagada y finalizada
    Cancelled = 4       // Cancelada por admin
}

/// <summary>
/// Tipo de operación energética
/// </summary>
public enum OperationType
{
    GENERATE = 0,  // Generación de energía
    CONSUME = 1    // Consumo de energía
}

/// <summary>
/// Información de un participante registrado
/// </summary>
public class ParticipantInfo
{
    public string Address { get; set; } = string.Empty;
    public bool IsRegistered { get; set; }
    public ParticipantType Type { get; set; }
    public BigInteger Balance { get; set; }
}

/// <summary>
/// Actividad comunitaria
/// </summary>
public class Activity
{
    public BigInteger Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public BigInteger Reward { get; set; }
    public string Executor { get; set; } = string.Empty;
    public ActivityState State { get; set; }
    public string[] ValidatorsSnapshot { get; set; } = new string[3];
    public byte Approvals { get; set; }
}

/// <summary>
/// Estado del dashboard
/// </summary>
public class DashboardState
{
    public string ConnectedAddress { get; set; } = string.Empty;
    public int ChainId { get; set; }
    public bool IsConnected { get; set; }
    public ParticipantInfo? ParticipantInfo { get; set; }
    public BigInteger EnergonBalance { get; set; }
    public BigInteger TotalSupply { get; set; }
    public BigInteger FundBalance { get; set; }
    public string[] CurrentValidators { get; set; } = new string[3];
    public string SmartMeterAddress { get; set; } = string.Empty;
    public BigInteger MeterNonce { get; set; }
    public bool IsSmartMeterAuthorized { get; set; }
}

/// <summary>
/// Respuesta de firma de la API SmartMeter
/// </summary>
public class SignatureApiResponse
{
    public bool Success { get; set; }
    public string Operation { get; set; } = string.Empty;
    public SignatureData Data { get; set; } = new();
    public ContractCallInfo ContractCall { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class SignatureData
{
    public string Participant { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public int Nonce { get; set; }
    public int OperationType { get; set; }
    public string Signature { get; set; } = string.Empty;
    public string MeterAddress { get; set; } = string.Empty;
}

public class ContractCallInfo
{
    public string Function { get; set; } = string.Empty;
    public List<object> Parameters { get; set; } = new();
}

/// <summary>
/// Evento de energía generada
/// </summary>
public class EnergyGeneratedEvent
{
    public string Participant { get; set; } = string.Empty;
    public BigInteger Amount { get; set; }
    public string Meter { get; set; } = string.Empty;
    public BigInteger ToParticipant { get; set; }
    public BigInteger ToFund { get; set; }
    public BigInteger BlockNumber { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Evento de energía consumida
/// </summary>
public class EnergyConsumedEvent
{
    public string Participant { get; set; } = string.Empty;
    public BigInteger Amount { get; set; }
    public string Meter { get; set; } = string.Empty;
    public BigInteger BlockNumber { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Evento de transferencia de token
/// </summary>
public class TransferEvent
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public BigInteger Value { get; set; }
    public BigInteger BlockNumber { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Respuesta de transacción
/// </summary>
public class TransactionResponse
{
    public bool Success { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
