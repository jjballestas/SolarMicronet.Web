// SolarMicronet - MetaMask Integration JavaScript
// Este archivo contiene todas las funciones necesarias para interactuar con MetaMask y los contratos

window.Web3Utils = {
    // ====================
    // CONFIGURACIÓN
    // ====================
    EXPECTED_CHAIN_ID: '0x539', // 1337 en hexadecimal
    CHAIN_NAME: 'BLOCK-LAB',
    ENERGON_TOKEN_ADDRESS: '0x9EB2074A0a4038f5A5e8a03d64B0EA9031159882',
    MICROGRID_MANAGER_ADDRESS: '0xC63Dec757Bc85D78117320c2BC3Cc580989CbAFd',

    // Estado interno
    currentAccount: null,
    currentChainId: null,

    // ====================
    // FUNCIONES DE CONEXIÓN
    // ====================
    
    isMetaMaskInstalled: function() {
        return typeof window.ethereum !== 'undefined';
    },

    connectWallet: async function(dotNetHelper) {
        if (!this.isMetaMaskInstalled()) {
            await dotNetHelper.invokeMethodAsync('OnWalletError', 'MetaMask no está instalado');
            return null;
        }

        try {
            const accounts = await window.ethereum.request({
                method: 'eth_requestAccounts'
            });

            if (accounts.length === 0) {
                await dotNetHelper.invokeMethodAsync('OnWalletError', 'No hay cuentas disponibles');
                return null;
            }

            this.currentAccount = accounts[0];
            const chainId = await window.ethereum.request({ method: 'eth_chainId' });
            this.currentChainId = chainId;

            this.setupListeners(dotNetHelper);
            await dotNetHelper.invokeMethodAsync('OnWalletConnected', this.currentAccount, parseInt(chainId, 16));
            return this.currentAccount;

        } catch (error) {
            console.error('Error connecting wallet:', error);
            await dotNetHelper.invokeMethodAsync('OnWalletError', error.message);
            return null;
        }
    },

    setupListeners: function(dotNetHelper) {
        if (!window.ethereum) return;

        window.ethereum.on('accountsChanged', async (accounts) => {
            if (accounts.length === 0) {
                this.currentAccount = null;
                await dotNetHelper.invokeMethodAsync('OnWalletDisconnected');
            } else {
                this.currentAccount = accounts[0];
                await dotNetHelper.invokeMethodAsync('OnAccountChanged', accounts[0]);
            }
        });

        window.ethereum.on('chainChanged', async (chainId) => {
            this.currentChainId = chainId;
            await dotNetHelper.invokeMethodAsync('OnChainChanged', parseInt(chainId, 16));
        });
    },

    getCurrentAccount: async function() {
        if (!this.isMetaMaskInstalled()) return null;
        try {
            const accounts = await window.ethereum.request({ method: 'eth_accounts' });
            return accounts.length > 0 ? accounts[0] : null;
        } catch (error) {
            console.error('Error getting current account:', error);
            return null;
        }
    },

    getChainId: async function() {
        if (!this.isMetaMaskInstalled()) return null;
        try {
            const chainId = await window.ethereum.request({ method: 'eth_chainId' });
            return parseInt(chainId, 16);
        } catch (error) {
            console.error('Error getting chainId:', error);
            return null;
        }
    },

    // ====================
    // FUNCIONES DE TRANSACCIONES
    // ====================

    sendTransaction: async function(to, data, value = '0x0') {
        if (!this.currentAccount) {
            throw new Error('Wallet no conectada');
        }

        try {
            const transactionParameters = {
                from: this.currentAccount,
                to: to,
                data: data,
                value: value
            };

            const txHash = await window.ethereum.request({
                method: 'eth_sendTransaction',
                params: [transactionParameters]
            });

            console.log('Transaction sent:', txHash);
            return txHash;
        } catch (error) {
            console.error('Error sending transaction:', error);
            throw error;
        }
    },

    // Generar energía
    generateEnergy: async function(participant, amount, nonce, signature, meterAddress) {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[
                {"internalType":"address","name":"participant","type":"address"},
                {"internalType":"uint256","name":"amount","type":"uint256"},
                {"internalType":"uint256","name":"nonce","type":"uint256"},
                {"internalType":"bytes","name":"signature","type":"bytes"},
                {"internalType":"address","name":"meter","type":"address"}
            ],
            "name":"generateEnergy",
            "outputs":[],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.MICROGRID_MANAGER_ADDRESS);
        const data = contract.methods.generateEnergy(participant, amount.toString(), nonce.toString(), signature, meterAddress).encodeABI();
        return await this.sendTransaction(this.MICROGRID_MANAGER_ADDRESS, data);
    },

    // Consumir energía
    consumeEnergy: async function(participant, amount, nonce, signature, meterAddress) {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[
                {"internalType":"address","name":"participant","type":"address"},
                {"internalType":"uint256","name":"amount","type":"uint256"},
                {"internalType":"uint256","name":"nonce","type":"uint256"},
                {"internalType":"bytes","name":"signature","type":"bytes"},
                {"internalType":"address","name":"meter","type":"address"}
            ],
            "name":"consumeEnergySigned",
            "outputs":[],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.MICROGRID_MANAGER_ADDRESS);
        const data = contract.methods.consumeEnergySigned(participant, amount.toString(), nonce.toString(), signature, meterAddress).encodeABI();
        return await this.sendTransaction(this.MICROGRID_MANAGER_ADDRESS, data);
    },

    // Transferir Energon
    transferEnergon: async function(to, amount) {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[
                {"internalType":"address","name":"to","type":"address"},
                {"internalType":"uint256","name":"value","type":"uint256"}
            ],
            "name":"transfer",
            "outputs":[{"internalType":"bool","name":"","type":"bool"}],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.ENERGON_TOKEN_ADDRESS);
        const data = contract.methods.transfer(to, amount.toString()).encodeABI();
        return await this.sendTransaction(this.ENERGON_TOKEN_ADDRESS, data);
    },

    // Reclamar actividad
    claimActivity: async function(activityId) {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[{"internalType":"uint256","name":"activityId","type":"uint256"}],
            "name":"claimActivity",
            "outputs":[],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.MICROGRID_MANAGER_ADDRESS);
        const data = contract.methods.claimActivity(activityId.toString()).encodeABI();
        return await this.sendTransaction(this.MICROGRID_MANAGER_ADDRESS, data);
    },

    submitActivity: async function(activityId) {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[{"internalType":"uint256","name":"activityId","type":"uint256"}],
            "name":"submitActivity",
            "outputs":[],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.MICROGRID_MANAGER_ADDRESS);
        const data = contract.methods.submitActivity(activityId.toString()).encodeABI();
        return await this.sendTransaction(this.MICROGRID_MANAGER_ADDRESS, data);
    },

    approveActivity: async function(activityId) {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[{"internalType":"uint256","name":"activityId","type":"uint256"}],
            "name":"approveActivity",
            "outputs":[],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.MICROGRID_MANAGER_ADDRESS);
        const data = contract.methods.approveActivity(activityId.toString()).encodeABI();
        return await this.sendTransaction(this.MICROGRID_MANAGER_ADDRESS, data);
    },

    processPayment: async function(activityId) {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[{"internalType":"uint256","name":"activityId","type":"uint256"}],
            "name":"processPayment",
            "outputs":[],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.MICROGRID_MANAGER_ADDRESS);
        const data = contract.methods.processPayment(activityId.toString()).encodeABI();
        return await this.sendTransaction(this.MICROGRID_MANAGER_ADDRESS, data);
    },

    // ====================
    // FUNCIONES DE ADMIN
    // ====================

    registerParticipant: async function(participantAddress, participantType) {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[
                {"internalType":"address","name":"participant","type":"address"},
                {"internalType":"uint8","name":"pType","type":"uint8"}
            ],
            "name":"registerParticipant",
            "outputs":[],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.MICROGRID_MANAGER_ADDRESS);
        const data = contract.methods.registerParticipant(participantAddress, participantType).encodeABI();
        return await this.sendTransaction(this.MICROGRID_MANAGER_ADDRESS, data);
    },

    authorizeMeter: async function(meterAddress, enabled) {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[
                {"internalType":"address","name":"meter","type":"address"},
                {"internalType":"bool","name":"enabled","type":"bool"}
            ],
            "name":"authorizeMeter",
            "outputs":[],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.MICROGRID_MANAGER_ADDRESS);
        const data = contract.methods.authorizeMeter(meterAddress, enabled).encodeABI();
        return await this.sendTransaction(this.MICROGRID_MANAGER_ADDRESS, data);
    },

    updateValidators: async function() {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[],
            "name":"updateValidators",
            "outputs":[],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.MICROGRID_MANAGER_ADDRESS);
        const data = contract.methods.updateValidators().encodeABI();
        return await this.sendTransaction(this.MICROGRID_MANAGER_ADDRESS, data);
    },

    createActivity: async function(description, reward) {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[
                {"internalType":"string","name":"description","type":"string"},
                {"internalType":"uint256","name":"reward","type":"uint256"}
            ],
            "name":"createActivity",
            "outputs":[],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.MICROGRID_MANAGER_ADDRESS);
        const data = contract.methods.createActivity(description, reward.toString()).encodeABI();
        return await this.sendTransaction(this.MICROGRID_MANAGER_ADDRESS, data);
    },

    cancelActivity: async function(activityId) {
        const web3 = new Web3(window.ethereum);
        const abi = [{
            "inputs":[{"internalType":"uint256","name":"activityId","type":"uint256"}],
            "name":"cancelActivity",
            "outputs":[],
            "stateMutability":"nonpayable",
            "type":"function"
        }];
        
        const contract = new web3.eth.Contract(abi, this.MICROGRID_MANAGER_ADDRESS);
        const data = contract.methods.cancelActivity(activityId.toString()).encodeABI();
        return await this.sendTransaction(this.MICROGRID_MANAGER_ADDRESS, data);
    }
};

console.log('SolarMicronet Web3Utils loaded successfully');
