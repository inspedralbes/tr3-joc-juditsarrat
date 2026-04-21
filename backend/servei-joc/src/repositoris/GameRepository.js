const IGameRepository = require('./IGameRepository');
const Game = require('../models/Game');

// Implementació del repositori de partides usant MongoDB i Mongoose.

class GameRepository extends IGameRepository {
    async findById(id) {
        return await Game.findById(id);
    }

    async findByCode(code) {
        // Si el codi proporcionat és un ID complet (24 caràcters), busquem per ID
        if (code && code.length === 24) {
            return await this.findById(code);
        }
        // Altrament, busquem per gameCode
        return await Game.findOne({ gameCode: code });
    }
    async findAll() {
        return await Game.find();
    }

    async findByStatus(status) {
        return await Game.find({ status: status });
    }

    async create(gameData) {
        // Si ja és una instància de Mongoose, la guardem directament
        if (gameData.save) {
            return await gameData.save();
        }
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
