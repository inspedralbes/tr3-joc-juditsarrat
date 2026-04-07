const IGameRepository = require('./IGameRepository');

//Implementació en memòria per a partides (molt útil per a tests).
//No usa funcions de fletxa ni mètodes avançats per simplicitat.

class GameRepositoryInMemory extends IGameRepository {
    constructor() {
        super();
        this.games = [];
    }

    async findById(id) {
        for (let i = 0; i < this.games.length; i++) {
            const game = this.games[i];
            if (game._id === id || game.id === id) {
                return game;
            }
        }
        return null;
    }

    async findAll() {
        return this.games;
    }

    async findByStatus(status) {
        const filtered = [];
        for (let i = 0; i < this.games.length; i++) {
            if (this.games[i].status === status) {
                filtered.push(this.games[i]);
            }
        }
        return filtered;
    }

    async create(gameData) {
        const id = "game_" + Date.now() + "_" + Math.floor(Math.random() * 1000);
        const newGame = {
            id: id,
            _id: id,
            hostId: gameData.hostId,
            status: gameData.status || 'waiting',
            players: gameData.players || [],
            config: gameData.config || {},
            createdAt: new Date(),
            updatedAt: new Date()
        };
        this.games.push(newGame);
        return newGame;
    }

    async update(id, gameData) {
        for (let i = 0; i < this.games.length; i++) {
            const game = this.games[i];
            if (game._id === id || game.id === id) {

                if (gameData.status) game.status = gameData.status;
                if (gameData.players) game.players = gameData.players;
                if (gameData.config) game.config = gameData.config;
                game.updatedAt = new Date();
                return game;
            }
        }
        return null;
    }

    async addPlayer(gameId, playerId) {
        for (let i = 0; i < this.games.length; i++) {
            const game = this.games[i];
            if (game._id === gameId || game.id === gameId) {

                let found = false;
                for (let j = 0; j < game.players.length; j++) {
                    if (game.players[j] === playerId) found = true;
                }
                if (!found) {
                    game.players.push(playerId);
                }
                game.updatedAt = new Date();
                return game;
            }
        }
        return null;
    }

    async updateStatus(gameId, status) {
        return await this.update(gameId, { status: status });
    }

    async delete(id) {
        for (let i = 0; i < this.games.length; i++) {
            if (this.games[i]._id === id || this.games[i].id === id) {
                const deleted = this.games.splice(i, 1);
                return deleted[0];
            }
        }
        return null;
    }
}

module.exports = GameRepositoryInMemory;
