const IGameRepository = require('./IGameRepository');
const Game = require('../models/Game');

// Implementació del repositori de partides usant MongoDB i Mongoose.

class GameRepository extends IGameRepository {
    async findById(id) {
        return await Game.findById(id);
    }

    async findAll() {
        return await Game.find();
    }

    async findByStatus(status) {
        return await Game.find({ status: status });
    }

    async create(gameData) {
        const newGame = new Game(gameData);
        return await newGame.save();
    }

    async update(id, gameData) {
        return await Game.findByIdAndUpdate(id, gameData, { new: true });
    }

    async addPlayer(gameId, playerId) {
        // Usem $addToSet per no afegir duplicats
        return await Game.findByIdAndUpdate(
            gameId,
            { $addToSet: { players: playerId } },
            { new: true }
        );
    }

    async updateStatus(gameId, status) {
        return await Game.findByIdAndUpdate(
            gameId,
            { status: status },
            { new: true }
        );
    }

    async delete(id) {
        return await Game.findByIdAndDelete(id);
    }
}

module.exports = GameRepository;
