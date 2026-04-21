const IResultRepository = require('./IResultRepository');
const Result = require('../models/Result');

/**
 * Implementació del repositori de resultats usant Mongoose.
 */
class ResultRepository extends IResultRepository {
    async findById(id) {
        return await Result.findById(id);
    }

    async findByGameId(gameId) {
        return await Result.findOne({ gameId: gameId });
    }

    async findByPlayerId(playerId) {
        return await Result.find({ "players.playerId": playerId });
    }

    async create(resultData) {
        const newResult = new Result(resultData);
        return await newResult.save();
    }
}

module.exports = ResultRepository;
