class JocService {
    constructor(gameRepository, webSocketService = null) {
        this.gameRepository = gameRepository;
        this.webSocketService = webSocketService;
    }

    async crearPartida(hostId, config) {
        const gameData = {
            hostId: hostId,
            players: [hostId],
            status: 'waiting',
            config: config || { maxPlayers: 2 }
        };

        // Crear instancia del modelo
        const Game = require('../models/Game');
        const game = new Game(gameData);

        // Generar gameCode con los últimos 6 caracteres de su propio _id
        game.gameCode = game._id.toString().slice(-6).toUpperCase();

        return await this.gameRepository.create(game);
    }

    async llistarPartidesDisponibles() {
        return await this.gameRepository.findByStatus('waiting');
    }

    async obtenirPerCodi(code) {
        const game = await this.gameRepository.findByCode(code);
        if (!game) {
            throw new Error("No s'ha trobat cap partida amb aquest codi.");
        }
        return game;
    }

    async unirsePartida(gameId, playerId) {
        const game = await this.gameRepository.findById(gameId);

        if (game === null) {
            throw new Error("La partida no existeix.");
        }
        if (game.status !== 'waiting') {
            throw new Error("La partida ja ha començat o ha finalitzat.");
        }

        let jaHiEs = false;
        for (let i = 0; i < game.players.length; i++) {
            if (game.players[i] === playerId) jaHiEs = true;
        }

        if (jaHiEs) {
            return game;
        }

        const maxPlayers = game.config.maxPlayers || 2;
        if (game.players.length >= maxPlayers) {
            throw new Error("La partida està plena.");
        }

        const updatedGame = await this.gameRepository.addPlayer(gameId, playerId);

        // ✅ NOTIFICAR POR WEBSOCKET
        if (this.webSocketService) {
            this.webSocketService.broadcast(gameId, {
                type: 'player-joined',
                payload: {
                    playerId: playerId,
                    totalPlayers: updatedGame.players.length
                }
            });
            console.log("✅ Notificación WebSocket enviada");
        }

        return updatedGame;
    }

    async iniciarPartida(gameId, hostId) {
        const game = await this.gameRepository.findById(gameId);
        if (game === null) {
            throw new Error("La partida no existeix.");
        }
        if (game.hostId !== hostId) {
            throw new Error("Només el host pot iniciar la partida.");
        }
        if (game.players.length < 2) {
            throw new Error("Es necessiten almenys 2 jugadors per començar.");
        }

        const updatedGame = await this.gameRepository.updateStatus(gameId, 'playing');

        // ✅ NOTIFICAR QUE LA PARTIDA HA COMENZADO
        if (this.webSocketService) {
            this.webSocketService.broadcast(gameId, {
                type: 'game-started',
                payload: { gameId: gameId }
            });
        }

        return updatedGame;
    }

    async processarAccio(gameId, playerId, accio) {
        const game = await this.gameRepository.findById(gameId);
        if (game === null) {
            throw new Error("La partida no existeix.");
        }
        if (game.status !== 'playing') {
            throw new Error("La partida no està en curs.");
        }
        console.log("Processant acció de " + playerId + " a la partida " + gameId + ":", accio);
        return game;
    }

    async finalitzarPartida(gameId) {
        return await this.gameRepository.updateStatus(gameId, 'finished');
    }

    async obtenirEstat(gameId) {
        const game = await this.gameRepository.findById(gameId);
        if (game === null) {
            throw new Error("La partida no existeix.");
        }
        return game;
    }
}

module.exports = JocService;