# SolarMicronet - Sistema de Gestión de Energía Descentralizada

## 📋 Descripción

Aplicación web ASP.NET Core Blazor Server para gestión descentralizada de energía en microrredes comunitarias usando blockchain.

## 🚀 Inicio Rápido

### Prerrequisitos
- .NET 8.0 SDK
- MetaMask
- Acceso a red BLOCK-LAB (ChainId 1337)

### Instalación

```bash
# 1. Restaurar dependencias
dotnet restore

# 2. Compilar
dotnet build

# 3. Ejecutar
dotnet run

# 4. Abrir navegador
http://localhost:5000
```

## 🏗️ Arquitectura

### Contratos Inteligentes
- **EnergonToken**: 0x9EB2074A0a4038f5A5e8a03d64B0EA9031159882
- **MicrogridManager**: 0xC63Dec757Bc85D78117320c2BC3Cc580989CbAFd  
- **SmartMeter**: 0xDbC1f6ee28C545ebd291D1D2d49646Bc834549eF

### SmartMeter API
- **Base URL**: https://smartmeterapi.ingenas.com/api

### Red Blockchain
- **Nombre**: BLOCK-LAB
- **ChainId**: 1337
- **RPC URL**: http://virtual.lab.inf.uva.es:60022

## 📁 Estructura del Proyecto

```
SolarMicronet.Web/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor        # Layout principal
│   ├── Pages/
│   │   ├── Home.razor              # Dashboard
│   │   ├── Energy.razor            # Operaciones energéticas
│   │   ├── Transfer.razor          # Transferencias
│   │   ├── Activities.razor        # Actividades comunitarias
│   │   ├── History.razor           # Historial
│   │   └── Admin.razor             # Administración
│   ├── App.razor
│   ├── Routes.razor
│   └── _Imports.razor
├── Models/
│   └── BlockchainModels.cs         # Modelos de datos
├── Services/
│   ├── BlockchainConfig.cs         # Configuración
│   ├── ContractABIs.cs             # ABIs
│   ├── MicrogridReadService.cs     # Lecturas blockchain
│   ├── SmartMeterApiClient.cs      # Cliente API
│   └── EventIndexerService.cs      # Eventos
├── wwwroot/js/
│   └── web3-integration.js         # MetaMask integration
├── Program.cs
├── appsettings.json
└── SolarMicronet.Web.csproj
```

## ✨ Funcionalidades

### 1. Dashboard
- Conexión MetaMask
- Balance de Energon
- Estado del participante
- Validadores actuales
- Fondo de reserva

### 2. Operaciones Energéticas
- Generar energía (Prosumers)
- Consumir energía
- Firma ECDSA con SmartMeter
- Nonce on-chain

### 3. Transferencias
- Transferencias ERC-20 de Energon
- Validación de balance y dirección

### 4. Actividades Comunitarias
- Crear (Admin)
- Reclamar
- Submit
- Aprobar (Validadores)
- Procesar pago (Quórum 2/3)

### 5. Historial
- Eventos de generación
- Eventos de consumo
- Transferencias

### 6. Administración
- Registrar participantes
- Autorizar Smart Meters
- Actualizar validadores
- Crear y cancelar actividades

## 🔧 Configuración MetaMask

```
Network Name: BLOCK-LAB
RPC URL: http://virtual.lab.inf.uva.es:60022
Chain ID: 1337
Currency Symbol: ETH
```

## 📊 Pipeline Técnico

### Generar/Consumir Energía
1. Usuario ingresa cantidad
2. UI llama SmartMeter API
3. API obtiene nonce on-chain
4. API genera firma ECDSA
5. API retorna {nonce, signature}
6. UI invoca MetaMask
7. Transacción al contrato
8. Actualización de estado

## 🔐 Seguridad

- Firma ECDSA (EIP-191)
- Nonce on-chain anti-replay
- Validación de roles
- Smart Meter autorizado

## 📚 Tecnologías

- ASP.NET Core 8.0
- Blazor Server
- Nethereum 4.19.0
- Bootstrap 5
- MetaMask + web3.js

## 👥 Roles

- **Consumer**: Consume y transfiere
- **Prosumer**: Genera, consume y transfiere  
- **Admin**: Gestión del sistema

## 📝 Notas

- Energon sin decimales ERC-20
- 1 ENRG = 1 kWh (conceptual)
- Comisión 1% a Fondo de Reserva
- Validadores = Top-3 por balance

## 📞 Soporte

Universidad de Valladolid - Master en Ingeniería de Sistemas
Curso: Distributed Ledger Technology (DLT)
