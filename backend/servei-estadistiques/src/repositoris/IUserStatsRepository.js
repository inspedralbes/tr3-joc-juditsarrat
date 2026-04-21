/**
 * Interfície per al repositori d'estadístiques d'usuari.
 */
class IUserStatsRepository {
    async findByUserId(userId) {
        throw new Error("Mètode findByUserId() no implementat.");
    }

    async updateStats(userId, statsData) {
        throw new Error("Mètode updateStats() no implementat.");
    }

    async create(statsData) {
        throw new Error("Mètode create() no implementat.");
    }

    async getTopPlayers(limit) {
        throw new Error("Mètode getTopPlayers() no implementat.");
    }
}

module.exports = IUserStatsRepository;
