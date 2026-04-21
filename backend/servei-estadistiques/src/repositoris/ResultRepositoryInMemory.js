const IResultRepository = require('./IResultRepository');

/**
 * Implementació en memòria per a resultats.
 */
class ResultRepositoryInMemory extends IResultRepository {
    constructor() {
        super();
        this.results = [];
    }

    async findById(id) {
        for (let i = 0; i < this.results.length; i++) {
            if (this.results[i]._id === id || this.results[i].id === id) {
                return this.results[i];
            }
        }
        return null;
    }

    async findByGameId(gameId) {
        for (let i = 0; i < this.results.length; i++) {
            if (this.results[i].gameId === gameId) {
                return this.results[i];
            }
        }
        return null;
    }

    async findByPlayerId(playerId) {
        const found = [];
        for (let i = 0; i < this.results.length; i++) {
            const res = this.results[i];
            let participant = false;
            for (let j = 0; j < res.players.length; j++) {
                if (res.players[j].playerId === playerId) participant = true;
            }
            if (participant) found.push(res);
        }
        return found;
    }

    async create(resultData) {
        const id = "res_" + Date.now() + "_" + Math.floor(Math.random() * 1000);
        const newResult = {
            id: id,
            _id: id,
            ...resultData,
            createdAt: new Date()
        };
        this.results.push(newResult);
        return newResult;
    }
}

module.exports = ResultRepositoryInMemory;
