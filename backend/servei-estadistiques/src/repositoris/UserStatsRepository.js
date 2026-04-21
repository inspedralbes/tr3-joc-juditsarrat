const IUserStatsRepository = require('./IUserStatsRepository');
const UserStats = require('../models/UserStats');

/**
 * Implementació del repositori d'estadístiques usant Mongoose.
 */
class UserStatsRepository extends IUserStatsRepository {
    async findByUserId(userId) {
        return await UserStats.findOne({ userId: userId });
    }

    async updateStats(userId, statsData) {
        return await UserStats.findOneAndUpdate(
            { userId: userId },
            { $set: statsData },
            { new: true, upsert: true }
        );
    }

    async create(statsData) {
        const newStats = new UserStats(statsData);
        return await newStats.save();
    }

    async getTopPlayers(limit) {
        return await UserStats.find()
            .sort({ wins: -1, totalScore: -1 })
            .limit(limit || 10);
    }
}

module.exports = UserStatsRepository;
